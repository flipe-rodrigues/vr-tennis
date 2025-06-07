# %%
# Imports
"""
.####.##.....##.########...#######..########..########..######.
..##..###...###.##.....##.##.....##.##.....##....##....##....##
..##..####.####.##.....##.##.....##.##.....##....##....##......
..##..##.###.##.########..##.....##.########.....##.....######.
..##..##.....##.##........##.....##.##...##......##..........##
..##..##.....##.##........##.....##.##....##.....##....##....##
.####.##.....##.##.........#######..##.....##....##.....######.
"""
import numpy as np
import matplotlib.pyplot as plt

# %%
# Helper functions (following Robinson & Robinson 2018)
"""
.##.....##.########.##.......########..########.########...######.
.##.....##.##.......##.......##.....##.##.......##.....##.##....##
.##.....##.##.......##.......##.....##.##.......##.....##.##......
.#########.######...##.......########..######...########...######.
.##.....##.##.......##.......##........##.......##...##.........##
.##.....##.##.......##.......##........##.......##....##..##....##
.##.....##.########.########.##........########.##.....##..######.
"""


def drag_coefficient(V, W):
    Cd = 0.6204 - 9.76e-4 * (V - 50) + (1.027e-4 - 2.24e-6 * (V - 50)) * W
    return Cd


def lift_coefficient(V, W):
    Cl = (4.68e-4 - 2.0984e-5 * (V - 50)) * W
    return Cl


def spin_decay_rate(V):
    w_lambda = 1 if V == 0 else np.exp(-dt / (164 / V))
    return w_lambda


def hollow_sphere_inertia(m, R, r):
    return (2 / 5) * m * (R**5 - r**5) / (R**3 - r**3)


def cross_sectional_area(R):
    return np.pi * R**2


# %%
# Physical constants
"""
..######...#######..##....##..######..########....###....##....##.########..######.
.##....##.##.....##.###...##.##....##....##......##.##...###...##....##....##....##
.##.......##.....##.####..##.##..........##.....##...##..####..##....##....##......
.##.......##.....##.##.##.##..######.....##....##.....##.##.##.##....##.....######.
.##.......##.....##.##..####.......##....##....#########.##..####....##..........##
.##....##.##.....##.##...###.##....##....##....##.....##.##...###....##....##....##
..######...#######..##....##..######.....##....##.....##.##....##....##.....######.
"""

# ball constants
m = 0.057  # mass of tennis ball (kg)
l = 0.006  # thickness of tennis ball shell (m)
R = 0.033  # outer radius of tennis ball (m)
r = R - l / 2  # inner radius of tennis ball (m)
I = hollow_sphere_inertia(m, R, r)  # moment of inertia (kg m^2)
A = cross_sectional_area(R)  # cross-sectional area (m^2)
ey = 0.75  # coefficient of restitution
beta = I / (m * R**2)

# environmental constants
g = 9.81  # gravity (m/s^2)
rho = 1.225  # air density (kg/m^3)
mu = 0.6  # coefficient of friction

# %%
# Simulation function
"""
..######..####.##.....##.##.....##.##..........###....########.########
.##....##..##..###...###.##.....##.##.........##.##......##....##......
.##........##..####.####.##.....##.##........##...##.....##....##......
..######...##..##.###.##.##.....##.##.......##.....##....##....######..
.......##..##..##.....##.##.....##.##.......#########....##....##......
.##....##..##..##.....##.##.....##.##.......##.....##....##....##......
..######..####.##.....##..#######..########.##.....##....##....########
"""


def simulate_trajectory(
    x0,
    y0,
    vx0,
    vy0,
    w0,
    dt=0.001,
    T=2.0,
    weights={"drag": 1, "magnus": 1, "gravity": 1},
):
    x = x0
    y = y0
    vx = vx0
    vy = vy0
    v = np.array([vx, vy])  # Initial velocity vector
    w = w0
    V = np.linalg.norm(v)
    W = np.linalg.norm(w)
    Cd = drag_coefficient(V, W)
    Cl = lift_coefficient(V, W)

    xs = [x]
    ys = [y]
    vxs = [vx]
    vys = [vy]
    Vs = [V]
    ws = [w]
    Cds = [Cd]
    Cls = [Cl]
    exs = [0]

    for _ in np.arange(0, T, dt):

        # Drag force
        Cd = drag_coefficient(V, W)
        Fd = -0.5 * rho * A * Cd * V * v

        # Magnus force (perpendicular to velocity, 2D: use cross product with z-axis)
        Cl = lift_coefficient(V, W)
        if W == 0:
            Fm = np.array([0, 0])
        else:
            Fm = 0.5 * rho * A * Cl * V * w * np.array([vy, -vx]) / W

        # Buoyancy force
        Fb = 4 / 3 * np.pi * R**3 * rho * np.array([0, g])

        # Gravity
        Fg = np.array([0, -g * m])

        # Total acceleration
        a = (
            weights["drag"] * Fd
            + weights["magnus"] * Fm
            + weights["buoyancy"] * Fb
            + weights["gravity"] * Fg
        ) / m
        ax = a[0]
        ay = a[1]

        # Update spin
        w *= spin_decay_rate(V)

        # Save initial velocities and spin before potential collision
        vx_i = vx
        vy_i = vy
        w_i = w

        # Update velocity and position
        vx += ax * dt
        vy += ay * dt

        # Update position
        x += vx * dt
        y += vy * dt

        # Bounce check (following Cross 2005)
        if y <= R and vy < 0:

            # Reset position to ground level
            y = R

            # Update vertical velocity
            vy = -ey * vy_i

            # Update horizontal velocity
            vx = 0.65 * vx_i + 0.3 * R * w_i

            # Update spin
            w = 0.4 * w_i + 0.58 * vx_i / R

        # Update velocity vector and speed
        v = np.array([vx, vy])
        V = np.linalg.norm(v)
        W = np.linalg.norm(w)

        xs.append(x)
        ys.append(y)
        vxs.append(vx)
        vys.append(vy)
        Vs.append(V)
        ws.append(w)
        Cds.append(Cd)
        Cls.append(Cl)
        exs.append(vx / vx_i)

    return (
        np.array(xs),
        np.array(ys),
        np.array(vxs),
        np.array(vys),
        np.array(Vs),
        np.array(ws),
        np.array(Cds),
        np.array(Cls),
        np.array(exs),
    )


# %%
# Contrasting aerodynamic forces
"""
....###....########.########...#######.
...##.##...##.......##.....##.##.....##
..##...##..##.......##.....##.##.....##
.##.....##.######...########..##.....##
.#########.##.......##...##...##.....##
.##.....##.##.......##....##..##.....##
.##.....##.########.##.....##..#######.
"""

# Example usage
x0 = 0  # initial horizontal position (m)
y0 = 1  # initial height (m)
vx0 = 10  # initial horizontal speed (m/s)
vy0 = 5  # initial vertical speed (m/s)
w0 = -100  # spin (rad/s)
dt = 0.001  # time step (s)
T = 5.0  # maximum time (s)

weight_combos = {
    "gravity": {"drag": 0, "magnus": 0, "buoyancy": 0, "gravity": 1},
    "gravity + drag": {"drag": 1, "magnus": 0, "buoyancy": 0, "gravity": 1},
    "gravity + magnus": {"drag": 0, "magnus": 1, "buoyancy": 0, "gravity": 1},
    "gravity + buoyancy": {"drag": 0, "magnus": 0, "buoyancy": 1, "gravity": 1},
    "all": {"drag": 1, "magnus": 1, "buoyancy": 1, "gravity": 1},
}
num_combos = len(weight_combos)
fig, axs = plt.subplots(6, 1, figsize=(10, 22))

# Simulate trajectory with different weights
for idx in np.arange(num_combos):
    weight_combo = list(weight_combos.keys())[idx]
    weights = weight_combos[weight_combo]

    # Simulate trajectory
    xs, ys, vxs, vys, Vs, ws, Cds, Cls, exs = simulate_trajectory(
        x0, y0, vx0, vy0, w0, dt, T, weights
    )
    t = np.arange(0, len(xs)) * dt

    axs[0].plot(t, xs, label=f"{weight_combo}")
    axs[1].plot(t, ys, label=f"{weight_combo}")
    axs[2].plot(t, vxs, label=f"{weight_combo}: vx")
    axs[3].plot(t, vys, label=f"{weight_combo}: vy")
    # axs[3].plot(t, np.abs(vys), label=f"{weight_combo}: |vy|")
    axs[4].plot(t, ws, label=f"{weight_combo}")
    axs[5].plot(xs, ys, label=f"{weight_combo}")

axs[0].set_ylabel("X Position (m)")
axs[0].legend()
axs[0].grid()

axs[1].set_ylabel("Height (m)")
axs[1].legend()
axs[1].grid()

axs[2].set_ylabel("Horizontal velocity (m/s)")
axs[2].legend()
axs[2].grid()

axs[3].set_ylabel("Vertical velocity (m/s)")
axs[3].legend()
axs[3].grid()

axs[4].set_xlabel("Time (s)")
axs[4].set_ylabel("Spin (rad/s)")
axs[4].legend()
axs[4].grid()

axs[5].set_xlabel("X Position (m)")
axs[5].set_ylabel("Height (m)")
axs[5].set_title("X-Y Trajectory")
axs[5].legend()
axs[5].grid()

plt.tight_layout()
plt.show()

plt.figure(figsize=(10, 6))
plt.plot(exs)

# %%
# Contrasting spins
"""
..######..########..####.##....##..######.
.##....##.##.....##..##..###...##.##....##
.##.......##.....##..##..####..##.##......
..######..########...##..##.##.##..######.
.......##.##.........##..##..####.......##
.##....##.##.........##..##...###.##....##
..######..##........####.##....##..######.
"""

# Example usage
x0 = 0  # initial horizontal position (m)
y0 = 1  # initial height (m)
vx0 = 10  # initial horizontal speed (m/s)
vy0 = 5  # initial vertical speed (m/s)
w0 = [-500, -200, 0, 200, 500]  # spin (rad/s)
dt = 0.001  # time step (s)
T = 5.0  # maximum time (s)

num_combos = len(w0)
fig, axs = plt.subplots(6, 1, figsize=(10, 22))

# Get colors from cool colormap
colors = plt.cm.cool(np.linspace(0, 1, num_combos))

# Simulate trajectory with different spins
for idx in np.arange(num_combos):

    xs, ys, vxs, vys, Vs, ws, Cds, Cls, exs = simulate_trajectory(
        x0, y0, vx0, vy0, w0[idx], dt, T, weight_combos["all"]
    )
    t = np.arange(0, len(xs)) * dt

    axs[0].plot(t, xs, label=f"w_0={w0[idx]} rad/s", color=colors[idx])
    axs[1].plot(t, ys, label=f"w_0={w0[idx]} rad/s", color=colors[idx])
    axs[2].plot(t, vxs, label=f"w_0={w0[idx]} rad/s: vx", color=colors[idx])
    axs[3].plot(t, vys, label=f"w_0={w0[idx]} rad/s: vy", color=colors[idx])
    axs[4].plot(t, ws, label=f"w_0={w0[idx]} rad/s", color=colors[idx])
    axs[5].plot(xs, ys, label=f"w_0={w0[idx]} rad/s", color=colors[idx])

axs[0].set_ylabel("X Position (m)")
axs[0].legend()
axs[0].grid()

axs[1].set_ylabel("Height (m)")
axs[1].legend()
axs[1].grid()

axs[2].set_ylabel("Horizontal velocity (m/s)")
axs[2].legend()
axs[2].grid()

axs[3].set_ylabel("Vertical velocity (m/s)")
axs[3].set_xlabel("Time (s)")
axs[3].legend()

axs[4].set_xlabel("Time (s)")
axs[4].set_ylabel("Spin (rad/s)")
axs[4].legend()
axs[4].grid()

axs[5].set_xlabel("X Position (m)")
axs[5].set_ylabel("Height (m)")
axs[5].set_title("X-Y Trajectory")
axs[5].legend()
axs[5].grid()

plt.tight_layout()
plt.show()

# %%
plt.figure(figsize=(10, 6))
plt.plot(Vs, label="Velocity")
plt.plot(ws, label="Spin")
plt.legend()
plt.xlabel("Time (s)")
plt.ylabel("Magnitude")
plt.show()

plt.figure(figsize=(10, 6))
plt.plot(Cds, label="Drag Coefficient")
plt.plot(Cls, label="Lift Coefficient")
plt.legend()
plt.xlabel("Time (s)")
plt.ylabel("Coefficient")
plt.show()

# %%
"""
.########.########..######..########..######.
....##....##.......##....##....##....##....##
....##....##.......##..........##....##......
....##....######....######.....##.....######.
....##....##.............##....##..........##
....##....##.......##....##....##....##....##
....##....########..######.....##.....######.
"""
Vs_test = np.linspace(0, 100, 100)  # Test velocities
ws_test = np.linspace(-500, 500, 100)  # Test spins in rad/s

plt.figure(figsize=(10, 6))
plt.plot(Vs_test, drag_coefficient(Vs_test, 200), color="blue", label="Drag")
plt.plot(
    Vs_test,
    lift_coefficient(Vs_test, 200),
    color="orange",
    label="Lift",
)
plt.xlabel("Velocity (m/s)")
plt.ylabel("Coefficient")
plt.legend()
plt.show()

plt.figure(figsize=(10, 6))
plt.plot(ws_test, drag_coefficient(25, ws_test), color="blue", label="Drag")
plt.plot(
    ws_test,
    lift_coefficient(25, ws_test),
    color="orange",
    label="Lift",
)
plt.xlabel("Spin (rad/s)")
plt.ylabel("Drag Coefficient")
plt.legend()
plt.show()
