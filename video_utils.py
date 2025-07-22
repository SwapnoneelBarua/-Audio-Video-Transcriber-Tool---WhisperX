# video_utils.py

import os
import ffmpeg

def extract_audio(video_path, output_dir="outputs"):
    audio_path = os.path.join(output_dir, os.path.splitext(os.path.basename(video_path))[0] + ".wav")
    ffmpeg.input(video_path).output(audio_path, ac=1, ar='16k').run(overwrite_output=True)
    return audio_path

def burn_subtitles(video_path, subtitle_path, output_dir="outputs"):
    output_path = os.path.join(output_dir, f"subtitled_{os.path.basename(video_path)}")
    ffmpeg.input(video_path).output(output_path, vf=f"subtitles={subtitle_path}").run(overwrite_output=True)
    return output_path
