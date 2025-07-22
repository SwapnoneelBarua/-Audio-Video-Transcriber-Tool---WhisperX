# diarizer.py

import whisperx
from config import HF_AUTH_TOKEN

def run_diarization(audio_path, segments, device="cuda"):
    diarize_model = whisperx.DiarizationPipeline(use_auth_token=HF_AUTH_TOKEN, device=device)
    diarized_segments = diarize_model(audio_path, segments)

    return diarized_segments
