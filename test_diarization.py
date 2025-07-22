# test_diarization.py

import torch
import whisperx
from config import HF_AUTH_TOKEN

AUDIO_PATH = "audio_samples/sample.wav"  # Replace with your test file path
LANGUAGE = "en"            # Optional: manually set language

def test_diarization(audio_path):
    print("🚀 Loading WhisperX model...")
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = whisperx.load_model("medium", device)

    print("🎧 Transcribing...")
    result = model.transcribe(audio_path, language=LANGUAGE)
    segments = result["segments"]
    print("✅ Transcription completed!")

    print("🗣️ Running Diarization...")
    diarize_model = whisperx.DiarizationPipeline(use_auth_token=HF_AUTH_TOKEN, device=device)
    diarized_segments = diarize_model(audio_path, segments)

    print("✅ Diarization completed!")
    print("\n🔊 Speaker Segments:")
    for seg in diarized_segments:
        start = round(seg['start'], 2)
        end = round(seg['end'], 2)
        speaker = seg['speaker']
        text = seg['text'].strip()
        print(f"[{start}s - {end}s] ({speaker}): {text}")

if __name__ == "__main__":
    test_diarization(AUDIO_PATH)
