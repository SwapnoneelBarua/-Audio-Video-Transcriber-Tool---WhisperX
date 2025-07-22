# utils.py
import os
import csv
import json
from datetime import timedelta

def format_timestamp(seconds):
    td = timedelta(seconds=float(seconds))
    return str(td).split(".")[0].zfill(8).replace('.', ',')

def save_transcript_files(segments, export_format="srt"):
    out_path = f"outputs/output.{export_format.lower()}"
    os.makedirs("outputs", exist_ok=True)

    if export_format.lower() == "srt":
        with open(out_path, "w", encoding="utf-8") as f:
            for i, seg in enumerate(segments, 1):
                f.write(f"{i}\n")
                f.write(f"{format_timestamp(seg['start'])} --> {format_timestamp(seg['end'])}\n")
                f.write(f"{seg['text'].strip()}\n\n")

    elif export_format.lower() == "vtt":
        with open(out_path, "w", encoding="utf-8") as f:
            f.write("WEBVTT\n\n")
            for seg in segments:
                f.write(f"{format_timestamp(seg['start'])} --> {format_timestamp(seg['end'])}\n")
                f.write(f"{seg['text'].strip()}\n\n")

    elif export_format.lower() == "csv":
        with open(out_path, "w", newline='', encoding="utf-8") as f:
            writer = csv.DictWriter(f, fieldnames=["start", "end", "text"])
            writer.writeheader()
            writer.writerows(segments)

    elif export_format.lower() == "json":
        with open(out_path, "w", encoding="utf-8") as f:
            json.dump(segments, f, ensure_ascii=False, indent=2)

    return out_path
