using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Renderer 
{
    public GameObject unityGameObject;
    public Camera unityCamera;
    public Texture2D viewportTexture;
    public CPU cpu;
    public PPU ppu;
    public Registers registers;
    public Interrupt interrupts;
    public MMU mmu;
    public Status._Status status;

    // Viewport
    public int viewportWidth = 160;
    public int viewportHeight = 144;
    public byte[] viewportPixels = new byte[160 * 144 * 4];
    public Rect viewportRect;// = new Rect(0, 0, viewportWidth, viewportHeight);

    public int windowHeight;
    public int windowWidth;

    public Font font;

    public int framerate_time = 1000 / 60;
    public int startFrame;
    public int endFrame;
    private float startFrameTime;
    public Renderer(Status._Status status, CPU cpu, PPU gpu, Registers registers, Interrupt interrupts, MMU mmu)
    {
        this.cpu = cpu;
        this.ppu = gpu;
        this.registers = registers;
        this.mmu = mmu;
        this.interrupts = interrupts;
        this.status = status;
        this.viewportRect = new Rect(0, 0, viewportWidth, viewportHeight);
        init();
        draw_viewport();
        
    }

    public void init()
    {
        // Initialize your objects here
       
        unityCamera = Camera.main;
        viewportTexture = new Texture2D(viewportWidth, viewportHeight, TextureFormat.ARGB32, false);
        Color32[] newColor = new Color32[viewportWidth * viewportHeight];
        unityGameObject = GameObject.FindGameObjectWithTag("Screen");
        viewportTexture.SetPixels32(newColor);
        viewportTexture.Apply();
        Sprite material = unityGameObject.GetComponent<SpriteRenderer>().sprite; // Get the Material component
        material = Sprite.Create(viewportTexture, new Rect(0, 0, viewportTexture.width, viewportTexture.height), new Vector2(0.5f, 0.5f));
    
    }

    public void initWindow(int width, int height)
    {
        Screen.SetResolution(width, height, false);
       //set text 
    }
    void check_framerate()
    {
        float endFrameTime = Time.realtimeSinceStartup;
        float timeTook = (endFrameTime - startFrameTime) * 1000; // Convert to milliseconds

        // If the frame took less time than desired frame time, sleep to limit the frame rate
        if (timeTook < framerate_time)
        {
            float sleepTime = (framerate_time - timeTook) / 1000; // Convert back to seconds
            Time.timeScale = 0; // Pause the game
            System.Threading.Thread.Sleep((int)(sleepTime * 1000)); // Sleep in milliseconds
            Time.timeScale = 1; // Resume the game
        }

        // Record the start time of the next frame
        startFrameTime = Time.realtimeSinceStartup;
    }

    void draw_viewport()
    {
        Color32[] newColor = new Color32[ppu.framebuffer.Length]; 
        for (int i = 0; i < 144 * 160; i++)
        {
            Color color = ppu.framebuffer[i];
            newColor[i].r = color.r;
            newColor[i].g = color.g;
            newColor[i].b = color.b;
            newColor[i].a = color.a;

            
            //Array.Copy(color.colors, 0, viewportPixels, viewportPixels.First() + i * 4, color.colors.Length);

        }
        
        viewportTexture.SetPixels32(newColor);
        //viewportTexture.SetPixelData<byte>(viewportPixels, 0);
        viewportTexture.Apply();
        unityGameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(viewportTexture, new Rect(0, 0, viewportTexture.width, viewportTexture.height), new Vector2(0.5f, 0.5f));// Get the Material component

    }
    public void Render()
    {
        check_framerate();
        draw_viewport();
    }
}
