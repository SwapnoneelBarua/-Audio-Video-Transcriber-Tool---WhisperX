import os
import torch
import whisperx
from pyannote.audio import Pipeline
from config import HF_TOKEN

# 📁 Path to your audio sample
AUDIO_FOLDER = "audio_samples"
AUDIO_FILE = "sample.mp3"  # Change to sample.wav if needed
AUDIO_PATH = os.path.join(AUDIO_FOLDER, AUDIO_FILE)

# 📁 Output directory
OUTPUT_DIR = "output"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# ⚙️ Set device
DEVICE = "cuda" if torch.cuda.is_available() else "cpu"
print(f"⚙️  Using device: {DEVICE}")

# 🔁 Load WhisperX model
print("🔁 Loading WhisperX model...")
model = whisperx.load_model("large-v2", device=DEVICE)

# 📝 Transcribe
print(f"📝 Transcribing: {AUDIO_FILE}")
transcription_result = model.transcribe(AUDIO_PATH)

# 🧠 Align (for better word-level timestamps)
model_a, metadata = whisperx.load_align_model(language_code=transcription_result["language"], device=DEVICE)
aligned_result = whisperx.align(transcription_result["segments"], model_a, metadata, AUDIO_PATH, DEVICE)

# 🔐 Load pyannote speaker diarization pipeline
print("🗣️ Running speaker diarization...")

try:
    diarization_pipeline = Pipeline.from_pretrained("pyannote/speaker-diarization", use_auth_token=HF_TOKEN)
except Exception as e:
    print("❌ Error loading diarization pipeline:", e)
    print("➡️ Visit https://huggingface.co/pyannote/speaker-diarization and accept the conditions if needed.")
    exit(1)

# 🔊 Run diarization
diarization_result = diarization_pipeline(AUDIO_PATH)

# 🧩 Combine diarization + transcription
result_with_speakers = whisperx.assign_word_speakers(aligned_result["word_segments"], diarization_result)

# 📝 Save full output
output_path = os.path.join(OUTPUT_DIR, f"{os.path.splitext(AUDIO_FILE)[0]}_diarized.json")

import json
with open(output_path, "w", encoding="utf-8") as f:
    json.dump(result_with_speakers, f, indent=2, ensure_ascii=False)

print(f"✅ Diarized transcript saved to: {output_path}")

# 🔍 Optional: print preview
print("\n🧾 Preview:")
for entry in result_with_speakers[:5]:
    print(f"[{entry['start']:.2f}s - {entry['end']:.2f}s] {entry['speaker']}: {entry['word']}")
