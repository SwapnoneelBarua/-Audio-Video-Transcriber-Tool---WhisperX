# transcriber.py
import whisperx
import torch

DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

def get_supported_languages():
    return {
        "en": "English",
        "hi": "Hindi",
        "bn": "Bengali",
        "ta": "Tamil",
        "te": "Telugu",
        "mr": "Marathi",
        "fr": "French",
        "es": "Spanish",
        "de": "German",
        "it": "Italian",
        "zh": "Chinese"
    }

def transcribe_audio(audio_path, language=None, translate=False, on_progress=None):
    model = whisperx.load_model("large-v3", device=DEVICE)

    if on_progress:
        on_progress(0.2, "Transcribing audio...")

    result = model.transcribe(audio_path, language=language, task="translate" if translate else "transcribe")
    segments = result["segments"]

    try:
        if on_progress:
            on_progress(0.5, "Aligning timestamps...")

        model_a, metadata = whisperx.load_align_model(language_code=result["language"], device=DEVICE)
        result_aligned = whisperx.align(segments, model_a, metadata, audio_path, device=DEVICE)

        if on_progress:
            on_progress(1.0, "Done.")
        return result_aligned["segments"]

    except Exception as e:
        print("❌ Alignment failed:", e)
        if on_progress:
            on_progress(1.0, "Alignment failed. Returning raw transcription.")
        return segments
