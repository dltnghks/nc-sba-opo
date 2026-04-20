using UnityEngine;

public sealed class PrototypeAudioManager : MonoBehaviour
{
    private const int SampleRate = 44100;

    private AudioSource audioSource;
    private AudioClip paddleHitClip;
    private AudioClip brickHitClip;
    private AudioClip brickBreakClip;
    private AudioClip lifeLostClip;
    private AudioClip clearClip;
    private AudioClip failClip;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.16f;

        paddleHitClip = CreateTone("PaddleHit", 640f, 0.05f);
        brickHitClip = CreateTone("BrickHit", 440f, 0.06f);
        brickBreakClip = CreateTone("BrickBreak", 880f, 0.08f);
        lifeLostClip = CreateTone("LifeLost", 220f, 0.18f);
        clearClip = CreateTone("RoundClear", 980f, 0.2f);
        failClip = CreateTone("RoundFail", 180f, 0.24f);
    }

    public void PlayPaddleHit()
    {
        audioSource.PlayOneShot(paddleHitClip);
    }

    public void PlayBrickHit(bool destroyed)
    {
        audioSource.PlayOneShot(destroyed ? brickBreakClip : brickHitClip);
    }

    public void PlayLifeLost()
    {
        audioSource.PlayOneShot(lifeLostClip);
    }

    public void PlayRoundEnd(bool cleared)
    {
        audioSource.PlayOneShot(cleared ? clearClip : failClip);
    }

    private static AudioClip CreateTone(string clipName, float frequency, float durationSeconds)
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * durationSeconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float envelope = Mathf.Clamp01(1f - progress);
            samples[i] = Mathf.Sin((2f * Mathf.PI * frequency * i) / SampleRate) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
