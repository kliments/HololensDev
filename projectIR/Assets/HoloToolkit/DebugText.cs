using UnityEngine;

public class DebugText : MonoBehaviour
{
    /// <summary>
    /// Unity textmesh for rendering text.
    /// </summary>
    TextMesh textMesh;

    /// <summary>
    /// Counter for number of frames.
    /// </summary>
    int frames;

    /// <summary>
    /// Tracks FPS.
    /// </summary>
    float fps = 0;

    /// <summary>
    /// Tracks which whole second we are on.
    /// </summary>
    int SecondsPassed = 0;

    void Start()
    {
        textMesh = gameObject.GetComponent<TextMesh>();
    }

    void Update()
    {
        // This is a super crude method of determining FPS.
        // Basically, we count how many frames we get each second.
        frames++;

        int currentSecond = (int)Time.time;
        if (currentSecond != SecondsPassed)
        {
            fps = frames;
            frames = 0;
            SecondsPassed = currentSecond;
        }

        // Also show available speech commands.
        textMesh.text = string.Format("fps: {0}}", fps.ToString());
    }
}