# %%
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp

# %%
class TennisBallSimulator:
    def __init__(self):
        # Physical constants
        self.g = 9.81          # gravitational acceleration (m/s²)
        self.rho = 1.225       # air density (kg/m³)
        self.r = 0.0335        # tennis ball outer radius (m)
        self.shell_thickness = 0.006  # shell thickness = 6mm
        self.r_inner = self.r - self.shell_thickness  # inner radius (m)
        self.m = 0.057         # tennis ball mass (kg)
        self.Cd = 0.47         # drag coefficient
        self.Cm = 0.5          # Magnus coefficient
        self.e = 0.75          # coefficient of restitution
        self.mu = 0.7          # friction coefficient
        
        # Moment of inertia for hollow sphere with 6mm shell thickness
        # I = (2/5) * m * (r_outer^5 - r_inner^5) / (r_outer^3 - r_inner^3)
        r_o5 = self.r**5
        r_i5 = self.r_inner**5
        r_o3 = self.r**3
        r_i3 = self.r_inner**3
        
        self.I = (2/5) * self.m * (r_o5 - r_i5) / (r_o3 - r_i3)
        self.beta = self.I / (self.m * self.r**2)
        
    def forces(self, state):
        """Calculate forces acting on the ball during flight"""
        x, y, vx, vy, omega = state
        
        # Total velocity
        v_total = np.sqrt(vx**2 + vy**2)
        
        if v_total < 1e-10:  # Avoid division by zero
            return np.array([0, 0, 0])
        
        # Drag force
        Fd = 0.5 * self.rho * self.Cd * np.pi * self.r**2 * v_total
        Fdx = -Fd * (vx / v_total)
        Fdy = -Fd * (vy / v_total)
        
        # Magnus force (perpendicular to velocity)
        Fm = 0.5 * self.rho * self.Cm * np.pi * self.r**2 * v_total * omega * self.r
        Fmx = -Fm * (vy / v_total)  # perpendicular to velocity
        Fmy = Fm * (vx / v_total)
        
        # Accelerations
        ax = (Fdx + Fmx) / self.m
        ay = (Fdy + Fmy) / self.m - self.g
        a_omega = 0  # No air torque on smooth ball
        
        return np.array([ax, ay, a_omega])
    
    def equations_of_motion(self, t, state):
        """ODE system for ball motion"""
        x, y, vx, vy, omega = state
        
        # Calculate accelerations
        ax, ay, a_omega = self.forces(state)
        
        # Return derivatives [dx/dt, dy/dt, dvx/dt, dvy/dt, domega/dt]
        return np.array([vx, vy, ax, ay, a_omega])
    
    def bounce(self, state):
        """Handle bounce physics when ball hits ground"""
        x, y, vx, vy, omega = state
        
        # Normal component (vertical) - simple restitution
        vy_new = -self.e * vy
        
        # Tangential component (horizontal) - friction model
        v_rel = vx + omega * self.r  # relative velocity at contact point
        
        # Check sticking vs sliding condition
        if abs(v_rel) <= self.mu * abs(vy_new):
            # Sticking condition
            vx_new = (vx + omega * self.r) / (1 + self.beta)
            omega_new = omega - (vx + omega * self.r) / (self.r * (1 + self.beta))
        else:
            # Sliding condition
            vx_new = vx - self.mu * abs(vy_new) * np.sign(v_rel)
            omega_new = omega + (5 * self.mu * abs(vy_new) * np.sign(v_rel)) / (2 * self.r)
        
        return np.array([x, 0, vx_new, vy_new, omega_new])
    
    def simulate(self, initial_state, t_max=5.0, dt=0.001):
        """
        Simulate tennis ball trajectory
        
        Parameters:
        initial_state: [x0, y0, vx0, vy0, omega0]
        t_max: maximum simulation time
        dt: time step for output
        
        Returns:
        t: time array
        trajectory: array of [x, y, vx, vy, omega] at each time step
        bounces: list of bounce times
        """
        
        trajectory = []
        times = []
        bounces = []
        
        current_state = np.array(initial_state)
        current_time = 0
        
        while current_time < t_max:
            # Check if ball is on or below ground
            if current_state[1] <= 0 and current_state[3] < 0:  # y <= 0 and vy < 0
                # Bounce occurred
                current_state = self.bounce(current_state)
                current_state[1] = 0  # Ensure ball is exactly on ground
                bounces.append(current_time)
                
                # If ball has very low energy, stop simulation
                if abs(current_state[3]) < 0.1:  # very low vertical velocity
                    break
            
            # Integrate until next time step or until ball hits ground
            t_span = [current_time, min(current_time + dt, t_max)]
            
            # Event function to detect ground contact
            def ground_contact(t, y):
                return y[1]  # y position
            ground_contact.terminal = True
            ground_contact.direction = -1  # only when decreasing
            
            # Solve ODE
            if current_state[1] > 0:  # Only integrate if ball is above ground
                sol = solve_ivp(self.equations_of_motion, t_span, current_state,
                              events=ground_contact, dense_output=True,
                              rtol=1e-8, atol=1e-10)
                
                if sol.t_events[0].size > 0:  # Ground contact detected
                    # Ball hit ground during integration
                    t_contact = sol.t_events[0][0]
                    current_state = sol.y_events[0][0]
                    current_time = t_contact
                else:
                    # No ground contact, use final state
                    current_state = sol.y[:, -1]
                    current_time = sol.t[-1]
            else:
                current_time += dt
            
            # Record state
            trajectory.append(current_state.copy())
            times.append(current_time)
        
        return np.array(times), np.array(trajectory), bounces
    
    def plot_trajectory(self, times, trajectory, bounces=None, title="Tennis Ball Trajectory"):
        """Plot the ball trajectory"""
        fig, (ax1, ax2, ax3, ax4) = plt.subplots(4, 1, figsize=(12, 12))
        
        x, y, vx, vy, omega = trajectory.T
        
        # Position plot
        ax1.plot(x, y, 'b-', linewidth=2, label='Trajectory')
        ax1.axhline(y=0, color='brown', linewidth=3, label='Ground')
        ax1.set_xlabel('Horizontal Distance (m)')
        ax1.set_ylabel('Height (m)')
        ax1.set_title(f'{title} - Position')
        ax1.grid(True, alpha=0.3)
        ax1.legend()
        ax1.set_aspect('equal')
        
        # Mark bounces
        if bounces:
            for bounce_time in bounces:
                idx = np.argmin(np.abs(times - bounce_time))
                ax1.plot(x[idx], y[idx], 'ro', markersize=8)
        
        # Velocity plot
        ax2.plot(times, vx, 'r-', label='Horizontal Velocity')
        ax2.plot(times, vy, 'b-', label='Vertical Velocity')
        ax2.set_xlabel('Time (s)')
        ax2.set_ylabel('Velocity (m/s)')
        ax2.set_title('Velocity Components')
        ax2.grid(True, alpha=0.3)
        ax2.legend()
        
        # Mark bounces
        if bounces:
            for bounce_time in bounces:
                ax2.axvline(x=bounce_time, color='gray', linestyle='--', alpha=0.7)
        
        # Spin plot
        ax3.plot(times, omega, 'g-', linewidth=2, label='Angular Velocity')
        ax3.set_xlabel('Time (s)')
        ax3.set_ylabel('Spin Rate (rad/s)')
        ax3.set_title('Ball Spin')
        ax3.grid(True, alpha=0.3)
        ax3.legend()
        
        # Mark bounces
        if bounces:
            for bounce_time in bounces:
                ax3.axvline(x=bounce_time, color='gray', linestyle='--', alpha=0.7)
        
        # Height over time plot
        ax4.plot(times, y, 'purple', linewidth=2, label='Height')
        ax4.axhline(y=0, color='brown', linewidth=2, label='Ground')
        ax4.set_xlabel('Time (s)')
        ax4.set_ylabel('Height (m)')
        ax4.set_title('Height vs Time')
        ax4.grid(True, alpha=0.3)
        ax4.legend()
        
        # Mark bounces
        if bounces:
            for bounce_time in bounces:
                ax4.axvline(x=bounce_time, color='gray', linestyle='--', alpha=0.7)
        
        plt.tight_layout()
        plt.show()

# %%
# Example usage and demonstrations
if __name__ == "__main__":
    simulator = TennisBallSimulator()
    
    # Example 1: Basic trajectory with topspin
    print("Simulating tennis ball with topspin...")
    initial_state = [0, 1.5, 20, 10, 50]  # x, y, vx, vy, omega (rad/s)
    times, trajectory, bounces = simulator.simulate(initial_state, t_max=3.0)
    simulator.plot_trajectory(times, trajectory, bounces, "Tennis Ball with Topspin")
    
    # Example 2: Backspin trajectory
    print("\nSimulating tennis ball with backspin...")
    initial_state = [0, 1.5, 20, 5, -50]  # negative omega = backspin
    times, trajectory, bounces = simulator.simulate(initial_state, t_max=4.0)
    simulator.plot_trajectory(times, trajectory, bounces, "Tennis Ball with Backspin")
    
    # Example 3: No spin trajectory (for comparison)
    print("\nSimulating tennis ball with no spin...")
    initial_state = [0, 1.5, 20, 10, 0]  # no spin
    times, trajectory, bounces = simulator.simulate(initial_state, t_max=3.0)
    simulator.plot_trajectory(times, trajectory, bounces, "Tennis Ball with No Spin")
    
    # Print some statistics
    print(f"\nSimulation completed:")
    print(f"Number of bounces: {len(bounces)}")
    print(f"Total flight time: {times[-1]:.2f} seconds")
    print(f"Maximum height: {np.max(trajectory[:, 1]):.2f} meters")
    print(f"Total horizontal distance: {trajectory[-1, 0]:.2f} meters")