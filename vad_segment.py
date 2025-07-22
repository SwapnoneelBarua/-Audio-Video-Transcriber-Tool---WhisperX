from pyannote.audio import Pipeline
import os

# Your Hugging Face access token
HF_TOKEN = "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"

# Load Voice Activity Detection pipeline with token
pipeline = Pipeline.from_pretrained("pyannote/segmentation", use_auth_token=HF_TOKEN)

# Set VAD hyperparameters
HYPER_PARAMETERS = {
    "onset": 0.6,     # activate speech above this probability
    "offset": 0.6,    # deactivate speech below this probability
    "min_duration_on": 0.1,   # minimum duration of speech segment
    "min_duration_off": 0.1,  # minimum duration of silence
}

pipeline.instantiate(HYPER_PARAMETERS)

# Path to audio file (must be mono WAV, 16kHz)
audio_file = "audio.wav"  # replace with your actual file path

# Apply VAD
vad = pipeline(audio_file)

# Save result
output_file = "vad_output.txt"
with open(output_file, "w") as f:
    for speech_turn in vad.get_timeline().support():
        start_time = speech_turn.start
        end_time = speech_turn.end
        f.write(f"{start_time:.2f} --> {end_time:.2f}\n")

print(f"[✓] VAD segments saved to {output_file}")
