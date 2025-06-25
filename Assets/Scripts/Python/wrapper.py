import subprocess
import os
from pathlib import Path


def run_stereotypy_analysis(subject_id):
    """Run stereotypy analysis for a specific subject"""
    try:
        # Convert notebook to python script and execute
        cmd = [
            "jupyter",
            "nbconvert",
            "--to",
            "script",
            "--execute",
            "stereotypy_analyses.ipynb",
            "--ExecutePreprocessor.kernel_name=python3",
        ]

        # Set environment variable for subject ID
        env = os.environ.copy()
        env["SUBJECT_ID"] = str(subject_id)

        result = subprocess.run(cmd, env=env, capture_output=True, text=True)

        if result.returncode == 0:
            print(f"Successfully processed subject {subject_id}")
        else:
            print(f"Error processing subject {subject_id}: {result.stderr}")

    except Exception as e:
        print(f"Exception processing subject {subject_id}: {str(e)}")


def main():
    # Define your subjects here - adapt based on your stereotypy_analyses.ipynb
    subjects = [
        "Joseph",
        "Francisco",
        "Manel",
        "Rodrigo",
        "Juan",
    ]  # Update with actual subject IDs

    # Alternative: read subjects from a file or directory
    # subjects = [d.name for d in Path('data').iterdir() if d.is_dir()]

    print(f"Processing {len(subjects)} subjects...")

    for subject in subjects:
        print(f"Processing subject: {subject}")
        run_stereotypy_analysis(subject)

    print("All subjects processed.")


if __name__ == "__main__":
    main()