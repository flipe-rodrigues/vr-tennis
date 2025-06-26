# %%
import os
import copy
import numpy as np
import pandas as pd
import matplotlib.colors as mcolors
import matplotlib.pyplot as plt
import plotly.graph_objs as go
from math import gcd
from plotly.subplots import make_subplots
from itertools import combinations
from sklearn.decomposition import PCA
from scipy.signal import resample_poly
from scipy.spatial import procrustes
from scipy.spatial.transform import Rotation as R
from scipy.spatial.distance import euclidean
from fastdtw import fastdtw

# %%
data_path = "C:\\Users\\flipe\\AppData\\LocalLow\\Warehouse\\VR-tennis\\Data\\Selected"
subject_paths = [f.name for f in os.scandir(data_path) if f.is_dir()]
subject_count = len(subject_paths)

# %%
save_path = "figures"

# %% [markdown]
# ### Iterate through all subjects
for subject2plot in subject_paths:
    print(f"\n{'='*50}")
    print(f"Processing subject: {subject2plot}")
    print(f"{'='*50}")

    # %%
    features2compare = [
        "centereyeanchor",
        "lefthandanchor",
        "racket-grip-base",
        "racket-stringbed-center",
        "racket-frame-left",
        "ball-tracker",
    ]
    features2compare_labels = [
        "head",
        "left-hand",
        "grip",
        "strings",
        "left-frame",
        "ball",
    ]
    feature_colors = [
        "tab:blue",
        "tab:orange",
        "tab:green",
        "tab:red",
        "tab:brown",
        "tab:purple",
    ]
    num_features = len(features2compare)

    # %%
    alignment = "RacketHit"
    window_duration = 0.5  # seconds
    half_window_duration = window_duration / 2
    original_fs = 500  # Hz
    target_fs = 120  # Hz

    # %%
    useHeadCentricDriftCorrection = True

    # %%
    """
    .##........#######.....###....########.
    .##.......##.....##...##.##...##.....##
    .##.......##.....##..##...##..##.....##
    .##.......##.....##.##.....##.##.....##
    .##.......##.....##.#########.##.....##
    .##.......##.....##.##.....##.##.....##
    .########..#######..##.....##.########.
    """
    # List CSV files in each subject directory
    for subject in subject_paths:
        subject_dir = os.path.join(data_path, subject)
        csv_files = [f for f in os.listdir(subject_dir) if f.endswith(".csv")]
        print(f"Subject: {subject}")
        print("CSV files:", csv_files)
        print("-" * 40)

    # Find the subject directory that matches subject2plot
    subject_dir_match = [s for s in subject_paths if subject2plot in s]
    if subject_dir_match:
        subject_dir = os.path.join(data_path, subject_dir_match[0])
        csv_files = [f for f in os.listdir(subject_dir) if f.endswith(".csv")]
        dfs_features = {}
        for feature in features2compare:
            csv_to_read = [f for f in csv_files if feature in f]
            if csv_to_read:
                csv_path = os.path.join(subject_dir, csv_to_read[0])
                label = features2compare_labels[features2compare.index(feature)]
                dfs_features[label] = pd.read_csv(csv_path)
                # dfs_features[label]["position.x"] *= -1
                print(f"Loaded {csv_to_read[0]} for subject {subject2plot}")
                print(dfs_features[label].head())
            else:
                print(
                    f"No CSV file found for feature: {feature} in subject {subject2plot}"
                )
    else:
        print(f"No subject directory found for: {subject2plot}")

    # %%
    all_stages = set()
    for label in dfs_features:
        if "stage" in dfs_features[label].columns:
            all_stages.update(dfs_features[label]["stage"].dropna().unique())

    unique_stages = sorted([int(s) for s in all_stages])
    num_stages = len(unique_stages)
    print(f"Unique stages: {unique_stages}")
    print(f"Number of unique stages: {num_stages}")

    # %%
    time2exclude = 60

    # Exclude data before a certain time for both features in dfs_features
    for label in dfs_features:
        mask = dfs_features[label]["time"] >= time2exclude
        dfs_features[label].loc[
            ~mask, dfs_features[label].columns.difference(["time"])
        ] = np.nan

    # %%
    dfs_trajectories_raw = {}

    # Iterate through each feature label to extract trajectories
    for label in features2compare_labels:
        df_feat = dfs_features[label]
        trajectories = []

        # Iterate through each unique stage
        for stage in unique_stages:
            df_stage = df_feat[df_feat["stage"] == stage]
            alignment_times = df_stage[df_stage["event"] == alignment]["time"].values

            # Iterate through each alignment time for the current stage
            for trial_idx, t_align in enumerate(alignment_times):

                # Get indices for the window around the hit
                if label in ["ball"]:
                    t_start = t_align
                    t_end = t_align + window_duration
                else:
                    t_start = t_align - half_window_duration
                    t_end = t_align + half_window_duration
                traj = df_stage[
                    (df_stage["time"] >= t_start) & (df_stage["time"] <= t_end)
                ][["position.x", "position.y", "position.z"]].to_numpy()
                traj_time = df_stage[
                    (df_stage["time"] >= t_start) & (df_stage["time"] <= t_end)
                ]["time"].to_numpy()

                # Downsample from 500 Hz to 120 Hz
                if len(traj_time) > 1:

                    # Calculate number of samples for target rate
                    num_samples = (
                        int(np.round((traj_time[-1] - traj_time[0]) * target_fs)) + 1
                    )
                    if num_samples > 1 and len(traj_time) > num_samples:
                        traj_time_ds = np.linspace(
                            traj_time[0], traj_time[-1], num_samples
                        )
                        traj_ds = np.column_stack(
                            [
                                np.interp(traj_time_ds, traj_time, traj[:, 0]),
                                np.interp(traj_time_ds, traj_time, traj[:, 1]),
                                np.interp(traj_time_ds, traj_time, traj[:, 2]),
                            ]
                        )
                    else:
                        traj_time_ds = traj_time
                        traj_ds = traj
                else:
                    traj_time_ds = traj_time
                    traj_ds = traj

                # Append the trajectory data to records
                trajectories.append(
                    {
                        "stage": stage,
                        "trial": trial_idx,
                        "time": traj_time_ds - t_align,
                        "x": traj_ds[:, 0],
                        "y": traj_ds[:, 1],
                        "z": traj_ds[:, 2],
                    }
                )

        # Store the trajectories in the dictionary as a DataFrame
        dfs_trajectories_raw[label] = pd.DataFrame(trajectories)

    # %%
    dfs_trajectories_drift = copy.deepcopy(dfs_trajectories_raw)

    # Iterate through each stage and feature to align trajectories
    for stage in unique_stages:
        for f_idx, label in enumerate(features2compare_labels):

            # Get all aligned trajectories for this feature and stage from dfs_trajectories
            df_feat_traj = dfs_trajectories_raw[label]
            df_ref_traj = dfs_trajectories_raw["head"]

            # Filter by stage
            feat_trajs_stage = df_feat_traj[df_feat_traj["stage"] == stage]
            ref_trajs_stage = df_ref_traj[df_ref_traj["stage"] == stage]

            # Iterate through each trajectory in the stage
            for i in range(len(feat_trajs_stage)):

                # Subtract the mean from the corresponding trajectory in the reference feature
                if label in [
                    "head",
                    "left-hand",
                    "grip",
                    "strings",
                    "left-frame",
                    "ball",
                ]:

                    # Get the i-th trajectory for feature and reference
                    feat_traj = feat_trajs_stage.iloc[i]
                    ref_traj = ref_trajs_stage.iloc[i]

                    # Subtract the mean of the reference trajectory from the feature trajectory
                    new_x = feat_traj["x"] - np.mean(ref_traj["x"])
                    new_y = feat_traj["y"] - np.mean(ref_traj["y"])
                    new_z = feat_traj["z"] - np.mean(ref_traj["z"])

                    # Update the original dfs_trajectories DataFrame
                    idx = feat_traj.name
                    dfs_trajectories_drift[label].at[idx, "x"] = new_x
                    dfs_trajectories_drift[label].at[idx, "y"] = new_y
                    dfs_trajectories_drift[label].at[idx, "z"] = new_z

    # %%
    """
    ..#######..########.
    .##.....##.##.....##
    ........##.##.....##
    ..#######..##.....##
    ........##.##.....##
    .##.....##.##.....##
    ..#######..########.
    """
    features2plot = [
        "head",
        "left-hand",
        "grip",
        "strings",
        # "left-frame",
        # "ball"
    ]

    fig = plt.figure(figsize=(16, 12))
    n_stages = len(unique_stages)
    for i, stage in enumerate(unique_stages):
        ax = fig.add_subplot(2, (n_stages + 1) // 2, i + 1, projection="3d")
        ax.set_title(f"Selected Features - Stage {stage}")
        ax.set_xlabel("X")
        ax.set_ylabel("Y")
        ax.set_zlabel("Z")

        for f_idx, label in enumerate(features2compare_labels):
            if label not in features2plot:
                continue
            df_traj = dfs_trajectories_drift[label]
            df_stage = df_traj[df_traj["stage"] == stage]
            for idx, row in df_stage.iterrows():
                x = np.array(row["x"])
                y = np.array(row["z"])
                z = np.array(row["y"])
                ax.plot(
                    x,
                    y,
                    z,
                    color=feature_colors[f_idx],
                    label=label if idx == df_stage.index[0] else None,
                    linewidth=1,
                    alpha=0.25,
                )
                # Mark the racket hit moment (time closest to zero)
                if "time" in row:
                    t = np.array(row["time"])
                    hit_idx = np.abs(t).argmin()
                    ax.plot(
                        x[hit_idx],
                        y[hit_idx],
                        z[hit_idx],
                        color=feature_colors[f_idx],
                        label="racket hit" if idx == df_stage.index[0] else None,
                        marker="x",
                        markersize=5,
                        alpha=0.25,
                    )
                    # Mark the first time point
                    ax.plot(
                        x[0],
                        y[0],
                        z[0],
                        color=feature_colors[f_idx],
                        label="swing start" if idx == df_stage.index[0] else None,
                        marker="o",
                        markersize=3,
                        alpha=0.25,
                    )

        handles, labels_ = ax.get_legend_handles_labels()
        by_label = dict(zip(labels_, handles))
        ax.legend(by_label.values(), by_label.keys())
        ax.view_init(elev=30, azim=-45)

    plt.tight_layout()
    
    # Save the figure
    fig_path = os.path.join(save_path, f"3dtrajectories_{subject2plot.lower()}.png")
    fig.savefig(fig_path, dpi=300, bbox_inches="tight")
    print(f"Figure saved to {fig_path}")
    plt.close(fig)

    # %%
    dfs_trajectories_norm = copy.deepcopy(
        dfs_trajectories_drift
        if useHeadCentricDriftCorrection
        else dfs_trajectories_raw
    )

    # normalization_denominator = "global_std"
    # normalization_denominator = "avg_speed"
    normalization_denominator = "avg_path_length"

    # Normalize each feature in dfs_trajectories using global mean and chosen denominator
    for label in dfs_trajectories_norm:
        df_traj = dfs_trajectories_norm[label]

        # Stack all position arrays to compute global mean and std (ignoring NaNs)
        all_positions = np.vstack(
            [
                np.column_stack([row["x"], row["y"], row["z"]])
                for _, row in df_traj.iterrows()
            ]
        )

        # Compute global mean position (ignoring NaNs)
        global_mean = np.nanmean(all_positions, axis=0)
        global_std = np.nanstd(all_positions)

        # Compute global average speed (Euclidean norm of velocity)
        all_speeds = []
        for _, row in df_traj.iterrows():
            positions = np.column_stack([row["x"], row["y"], row["z"]])
            times = row["time"]
            dt = np.diff(times)
            velocities = np.diff(positions, axis=0) / dt[:, None]
            speeds = np.linalg.norm(velocities, axis=1)
            all_speeds.append(speeds)
        global_avg_speed = np.nanmean(np.concatenate(all_speeds)) if all_speeds else 1.0

        # Compute global average path length
        all_path_lengths = []
        for _, row in df_traj.iterrows():
            positions = np.column_stack([row["x"], row["y"], row["z"]])
            diffs = np.diff(positions, axis=0)
            dists = np.linalg.norm(diffs, axis=1)
            path_length = np.nansum(dists)
            all_path_lengths.append(path_length)
        global_avg_path_length = (
            np.nanmean(all_path_lengths) if all_path_lengths else 1.0
        )

        # Choose denominator for normalization
        if normalization_denominator == "global_std":
            denominator = global_std
        elif normalization_denominator == "avg_speed":
            denominator = global_avg_speed
        elif normalization_denominator == "avg_path_length":
            denominator = global_avg_path_length
        else:
            raise ValueError(
                "normalization_denominator must be 'global_std', 'avg_speed', or 'avg_path_length'"
            )

        # Normalize each trajectory in the DataFrame
        for idx, row in df_traj.iterrows():
            for i, coord in enumerate(["x", "y", "z"]):
                df_traj.at[idx, coord] = (row[coord] - global_mean[i]) / denominator

    # %%
    """
    .########...######.....###...
    .##.....##.##....##...##.##..
    .##.....##.##........##...##.
    .########..##.......##.....##
    .##........##.......#########
    .##........##....##.##.....##
    .##.........######..##.....##
    """
    dfs_trajectories_pca = copy.deepcopy(dfs_trajectories_norm)

    # Dictionary to store PCA results for each feature
    pca_results = {}

    for label in dfs_trajectories_pca:
        df_traj = dfs_trajectories_pca[label]

        # Stack all trajectories for this feature: shape (n_trials * window_size, 3)
        all_positions = np.vstack(
            [
                np.column_stack([row["x"], row["y"], row["z"]])
                for _, row in df_traj.iterrows()
            ]
        )

        # Remove rows with NaNs
        valid_mask = ~np.isnan(all_positions).any(axis=1)
        pca_input = all_positions[valid_mask]

        # Fit PCA
        pca = PCA(n_components=3)
        pca.fit(pca_input)

        # Store PCA object and explained variance
        pca_results[label] = {
            "pca": pca,
            "explained_variance_ratio": pca.explained_variance_ratio_,
        }
        print(
            f"{label}:\n\tPC1={pca.explained_variance_ratio_[0]:.2%}, PC2={pca.explained_variance_ratio_[1]:.2%}, PC3={pca.explained_variance_ratio_[2]:.2%}"
        )

        # Project each trajectory onto the first three principal components and store in dfs_trajectories
        for idx, row in df_traj.iterrows():
            traj = np.column_stack([row["x"], row["y"], row["z"]])
            mask = ~np.isnan(traj).any(axis=1)
            pc_scores = np.full((traj.shape[0], 3), np.nan)
            if np.any(mask):
                pc_scores[mask] = pca.transform(traj[mask])
            df_traj.at[idx, "x"] = pc_scores[:, 0]
            df_traj.at[idx, "y"] = pc_scores[:, 1]
            df_traj.at[idx, "z"] = pc_scores[:, 2]

    # %%
    """
    ..######..##......##.####.##....##..######..
    .##....##.##..##..##..##..###...##.##....##.
    .##.......##..##..##..##..####..##.##.......
    ..######..##..##..##..##..##.##.##.##...####
    .......##.##..##..##..##..##..####.##....##.
    .##....##.##..##..##..##..##...###.##....##.
    ..######...###..###..####.##....##..######..
    """
    fig, axs = plt.subplots(
        num_stages,
        num_features,
        figsize=(4 * num_features, 4 * num_stages),
        sharex="col",
        sharey="col",
    )
    fig.suptitle(f"{subject2plot}", fontsize=14, x=0.05, y=0.99)

    for stage_idx, stage in enumerate(unique_stages):
        for i, label in enumerate(features2compare_labels):
            df_traj = dfs_trajectories_pca[label]

            # Filter by stage
            df_stage = df_traj[df_traj["stage"] == stage]
            for _, row in df_stage.iterrows():
                traj_pc1 = np.array(row["x"])
                traj_pc2 = np.array(row["y"])
                if traj_pc1.shape[0] > 0 and traj_pc2.shape[0] > 0:
                    axs[stage_idx, i].plot(
                        traj_pc1, traj_pc2, color="black", alpha=0.25, linewidth=1
                    )

                    # Mark the racket hit moment (center of window)
                    hit_idx = np.abs(row["time"]).argmin()
                    axs[stage_idx, i].plot(
                        traj_pc1[hit_idx],
                        traj_pc2[hit_idx],
                        color="red",
                        marker="o",
                        markersize=3,
                        alpha=0.25,
                        label="Racket Hit" if stage_idx == 0 and i == 0 else None,
                    )

                    # Mark the start of the trajectory
                    axs[stage_idx, i].plot(
                        traj_pc1[0],
                        traj_pc2[0],
                        color="black",
                        alpha=0.25,
                        marker="o",
                        markersize=3,
                    )

            axs[stage_idx, i].set_title(f"{label} - Stage {stage}")
            axs[stage_idx, i].set_xlabel("PC1")
            axs[stage_idx, i].set_ylabel("PC2")
            axs[stage_idx, i].grid()

    plt.tight_layout()

    # Save the figure
    fig_path = os.path.join(save_path, f"pcatrajectories_{subject2plot.lower()}.png")
    fig.savefig(fig_path, dpi=300, bbox_inches="tight")
    print(f"Figure saved to {fig_path}")
    plt.close(fig)

    # %%
    """
    .########..##.....##..######..########
    .##.....##.###...###.##....##.##......
    .##.....##.####.####.##.......##......
    .########..##.###.##..######..######..
    .##...##...##.....##.......##.##......
    .##....##..##.....##.##....##.##......
    .##.....##.##.....##..######..########
    """

    # Plot stereotypy metrics: MSE, RMSE, and IQR using dfs_trajectories_pca
    fig, axs = plt.subplots(
        num_stages, 3, figsize=(12, 4 * num_stages), sharex=True, sharey="col"
    )
    fig.suptitle(
        f"{subject2plot} - Trajectory Stereotypy Metrics", fontsize=14, x=0.5, y=0.99
    )

    for stage_idx, stage in enumerate(unique_stages):
        for f_idx, label in enumerate(features2compare_labels):
            df_traj = dfs_trajectories_pca[label]

            # Filter by stage
            df_stage = df_traj[df_traj["stage"] == stage]

            aligned_trajs = []
            for _, row in df_stage.iterrows():
                traj = np.column_stack([row["x"], row["y"], row["z"]])
                aligned_trajs.append(traj)

            aligned_trajs = np.array(aligned_trajs)  # shape: (n_trials, window_size, 3)
            if aligned_trajs.shape[0] == 0:
                continue

            # Extract the time vector for the current stage
            traj_time = row["time"]

            # Mean trajectory
            mean_traj = np.mean(aligned_trajs, axis=0)  # (window_size, 3)

            # MSE: mean squared error to the mean trajectory at each time
            mse = np.mean(
                np.sum((aligned_trajs - mean_traj) ** 2, axis=2), axis=0
            )  # (window_size,)

            # RMSE: root mean squared error to the mean trajectory at each time
            rmse = np.sqrt(mse)  # (window_size,)

            # IQR: interquartile range of Euclidean distances to the mean trajectory at each time
            dists = np.linalg.norm(
                aligned_trajs - mean_traj, axis=2
            )  # (n_trials, window_size)
            iqr = np.percentile(dists, 75, axis=0) - np.percentile(dists, 25, axis=0)

            axs[stage_idx, 0].plot(
                traj_time, mse, color=feature_colors[f_idx], label=label
            )
            axs[stage_idx, 1].plot(
                traj_time, rmse, color=feature_colors[f_idx], label=label
            )
            axs[stage_idx, 2].plot(
                traj_time, iqr, color=feature_colors[f_idx], label=label
            )

        axs[stage_idx, 0].set_title(f"Stage {stage} - MSE")
        axs[stage_idx, 0].set_ylabel("MSE")
        axs[stage_idx, 0].legend()
        axs[stage_idx, 0].grid()

        axs[stage_idx, 1].set_title(f"Stage {stage} - RMSE")
        axs[stage_idx, 1].set_ylabel("RMSE")
        axs[stage_idx, 1].legend()
        axs[stage_idx, 1].grid()

        axs[stage_idx, 2].set_title(f"Stage {stage} - IQR")
        axs[stage_idx, 2].set_ylabel("IQR (Euclidean dist.)")
        axs[stage_idx, 2].legend()
        axs[stage_idx, 2].grid()

    for ax in axs[-1, :]:
        ax.set_xlabel("Time since racket hit (s)")

    plt.tight_layout(rect=[0, 0, 1, 0.97])

    # Save the figure
    fig_path = os.path.join(save_path, f"rmse_{subject2plot.lower()}.png")
    fig.savefig(fig_path, dpi=300, bbox_inches="tight")
    print(f"Figure saved to {fig_path}")
    plt.close(fig)

    # %% [markdown]
    # ### Compute trajectory dissimilarity metrics
    """
    .########..########.##......##
    .##.....##....##....##..##..##
    .##.....##....##....##..##..##
    .##.....##....##....##..##..##
    .##.....##....##....##..##..##
    .##.....##....##....##..##..##
    .########.....##.....###..###.
    """
    dtw_distances = []
    procrustes_distances = []

    # Iterate through each stage and feature to compute distances
    for stage in unique_stages:
        for f_idx, label in enumerate(features2compare_labels):
            print(
                f"  Computing DTW and Procrustes distances for stage {stage}, feature '{label}'..."
            )
            df_traj = dfs_trajectories_pca[label]

            # Filter by stage
            df_stage = df_traj[df_traj["stage"] == stage]

            # Collect aligned trajectories (PC1, PC2, PC3)
            aligned_trajs = []
            for _, row in df_stage.iterrows():
                traj = np.column_stack([row["x"], row["y"], row["z"]])
                aligned_trajs.append(traj)
            aligned_trajs = np.array(aligned_trajs)  # shape: (n_trials, window_size, 3)

            if aligned_trajs.shape[0] < 2:
                continue

            # Compute distances for all unique pairs and store in DataFrames
            for idx1, idx2 in combinations(range(len(aligned_trajs)), 2):

                # DTW distance
                distance_dtw, _ = fastdtw(
                    aligned_trajs[idx1], aligned_trajs[idx2], dist=euclidean
                )
                dtw_distances.append(
                    {
                        "stage": stage,
                        "feature": label,
                        "trial1": idx1,
                        "trial2": idx2,
                        "distance": distance_dtw,
                    }
                )

                # Procrustes distance
                _, _, disparity = procrustes(aligned_trajs[idx1], aligned_trajs[idx2])
                procrustes_distances.append(
                    {
                        "stage": stage,
                        "feature": label,
                        "trial1": idx1,
                        "trial2": idx2,
                        "distance": disparity,
                    }
                )

    # Convert the lists of distances to DataFrames
    df_dtw_distances = pd.DataFrame(dtw_distances)
    df_procrustes_distances = pd.DataFrame(procrustes_distances)

    # %%
    """
    ..######..########..########..######.
    .##....##.##.....##.##.......##....##
    .##.......##.....##.##.......##......
    .##.......##.....##.######....######.
    .##.......##.....##.##.............##
    .##....##.##.....##.##.......##....##
    ..######..########..##........######.
    """
    fig, axs = plt.subplots(
        len(unique_stages),
        2,
        figsize=(10, 4 * len(unique_stages)),
        sharex="col",
        sharey=True,
    )
    fig.suptitle("CDFs of DTW and Procrustes Distances per Stage", fontsize=16, y=0.98)

    for stage_idx, stage in enumerate(unique_stages):
        for f_idx, label in enumerate(features2compare_labels):

            # DTW CDF
            mask_dtw = (df_dtw_distances["stage"] == stage) & (
                df_dtw_distances["feature"] == label
            )
            dtw_vals = df_dtw_distances.loc[mask_dtw, "distance"].values
            if len(dtw_vals) > 0:
                sorted_dtw = np.sort(dtw_vals)
                cdf_dtw = np.arange(1, len(sorted_dtw) + 1) / len(sorted_dtw)
                axs[stage_idx, 0].plot(
                    sorted_dtw, cdf_dtw, color=feature_colors[f_idx], label=label
                )

            # Procrustes CDF
            mask_proc = (df_procrustes_distances["stage"] == stage) & (
                df_procrustes_distances["feature"] == label
            )
            proc_vals = df_procrustes_distances.loc[mask_proc, "distance"].values
            if len(proc_vals) > 0:
                sorted_proc = np.sort(proc_vals)
                cdf_proc = np.arange(1, len(sorted_proc) + 1) / len(sorted_proc)
                axs[stage_idx, 1].plot(
                    sorted_proc, cdf_proc, color=feature_colors[f_idx], label=label
                )

        axs[stage_idx, 0].set_title(f"Stage {stage} - DTW CDF")
        axs[stage_idx, 0].set_xlabel("DTW Distance")
        axs[stage_idx, 0].set_ylabel("CDF")
        axs[stage_idx, 0].set_xlim([0, 25])
        axs[stage_idx, 0].legend()
        axs[stage_idx, 0].grid()

        axs[stage_idx, 1].set_title(f"Stage {stage} - Procrustes CDF")
        axs[stage_idx, 1].set_xlabel("Procrustes Distance")
        axs[stage_idx, 1].set_ylabel("CDF")
        axs[stage_idx, 1].set_xlim([0, 0.5])
        axs[stage_idx, 1].legend()
        axs[stage_idx, 1].grid()

    plt.tight_layout(rect=[0, 0, 1, 0.96])

    # Save the figure
    fig_path = os.path.join(save_path, f"dtw_{subject2plot.lower()}.png")
    fig.savefig(fig_path, dpi=300, bbox_inches="tight")
    print(f"Figure saved to {fig_path}")
    plt.close(fig)

    # %% [markdown]
    # ### Save data
    """
    ..######.....###....##.....##.########
    .##....##...##.##...##.....##.##......
    .##........##...##..##.....##.##......
    ..######..##.....##.##.....##.######..
    .......##.#########..##...##..##......
    .##....##.##.....##...##.##...##......
    ..######..##.....##....###....########
    """
    import pickle

    # Gather all variables in the current workspace
    workspace_vars = {
        "dfs_trajectories_pca": dfs_trajectories_pca,
        "df_dtw_distances": df_dtw_distances,
    }

    # Save to pickle file named after the subject
    pickle_filename = os.path.join("data", f"{subject2plot.lower()}.pkl")
    with open(pickle_filename, "wb") as f:
        pickle.dump(workspace_vars, f)

    print(f"Workspace saved to {pickle_filename}")
