import os
import subprocess

def extract_audio_from_video(video_path, output_audio_path="extracted_audio.wav"):
    try:
        command = [
            "ffmpeg",
            "-y",  # Overwrite if exists
            "-i", video_path,
            "-vn",  # No video
            "-acodec", "pcm_s16le",
            "-ar", "16000",
            "-ac", "1",
            output_audio_path
        ]
        subprocess.run(command, check=True)
        return output_audio_path
    except subprocess.CalledProcessError as e:
        print("Error during audio extraction:", e)
        return None
