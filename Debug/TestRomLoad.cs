using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestRomLoad : MonoBehaviour
{
   
    Registers registers;
    public Status._Status status;
    Cartridge newCart;
    MMU mmu;
    Interrupt interrupts;
    CPU cpu;
    public PPU ppu;
    Timer timer;
    Joypad joypad;
    GBRenderer gbRender;
    public string romName;
    public string bootRomName;
    public bool debug;
    public GameObject debugText;
    public GameObject debugTextOpCode;
    public bool auto = false;
    public int ticks = 1;
    public int frame = 1;
    public ushort checkAddress = 0xFF9C;
    string output;
    //DEBUG
    int spritemap_height = 64;
    int spritemap_width = 40;
    public byte[] spritemapPixels = new byte[64 * 40 * 4];
    public int viewportWidth = 40;
    public int viewportHeight = 64;
    public GameObject unityGameObject;
    public Camera unityCamera;
    public Texture2D viewportTexture;
    Color32[] colors = new Color32[64 * 40];
    // Start is called before the first frame update
    void Start()
    {
        registers = new Registers();
        newCart = new Cartridge();
        newCart._Cartridge(romName, "");
        mmu = new MMU(newCart, registers);
        // mmu.load_default_boot_rom();
        registers.debugLabel = debugText;
        interrupts = new Interrupt(registers, mmu);
        timer = new Timer(mmu, interrupts);
        status = new Status._Status
         {
             debug = false,
             isRunning = true,
             isPaused = false,
             doStep = false,
             soundEnabled = true,
             colorMode = (int)Status.ColorModes.NORMAL,
         };
        cpu = new CPU(registers, interrupts, mmu);
        cpu.no_bootrom_init();
        cpu.instructions.debugLabel = debugTextOpCode;
        ppu = new PPU(registers, interrupts, mmu);
        joypad = new Joypad(status, interrupts, mmu);
        gbRender = new GBRenderer(status, cpu, ppu, registers, interrupts, mmu);
        viewportTexture = new Texture2D(64, 40, TextureFormat.ARGB32, false);


    }

    public void DrawSpriteMap(MMU.Sprite sprite, int tile_off, int off_x, int off_y)
    {
        
        if (!sprite.ready)
            return;
        for (int x = 0; x < 8; x++)
        {
            byte xF = (byte)(sprite.xFlip != 0 ? 7 - x : x);
            for (int y = 0; y < 8; y++)
            {
                byte yF = (byte)(sprite.yFlip != 0 ? 7 - y : y);
                byte colour_n = mmu.tiles[sprite.tile + tile_off].pixels[yF][xF];
                int offsetX = ((off_x + x) % spritemap_width);
                int offsetY = y + off_y;
                int offset = (offsetY * spritemap_width + offsetX);

                Color colour = sprite.colourPalette[colour_n];
                colors[offset].r = colour.r;
                colors[offset].g = colour.g;
                colors[offset].b = colour.b;
                colors[offset].a = colour.a;

               
                //Array.Copy(colour.colors, 0, spritemapPixels, spritemapPixels.First() + offset, colour.colors.Length);

            }
        }
    }
    public void UpdateSpriteMap()
    {
       
        if (ppu.control.spriteSize != 0)
        {
            for (int i = 0, row = 0; i < 20; i++)
            {
                DrawSpriteMap(mmu._sprites[i], 0, i * 8, row * 8);
                DrawSpriteMap(mmu._sprites[i], 1, i * 8, row * 8 + 8);
                if (((i + 1) % 5) == 0)
                    row += 2;
            }
        }
        else
        {
            for (int i = 0, row = 0; i < 40; i++)
            {
                DrawSpriteMap(mmu._sprites[i], 0, i * 8, row * 8);
                if (((i + 1) % 5) == 0)
                    row++;
            }
        }

        viewportTexture.SetPixels32(colors);
       // viewportTexture.SetPixelData<byte>(spritemapPixels, 0);
        viewportTexture.Apply();
        unityGameObject = GameObject.FindGameObjectWithTag("Debug");
        unityGameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(viewportTexture, new Rect(0, 0, viewportTexture.width, viewportTexture.height), new Vector2(0.5f, 0.5f));// Get the Material component

    }
    public void DoStep()
    {
        if (debug)
        {
           
            registers.print_registers();
            // registers.print_flags();
            Debug.Log("Check Address 0x: " + mmu.memory[checkAddress].ToString("X2"));
            Debug.Log("Tima: "+ mmu._timer.tima);
            UpdateSpriteMap();
            
        }

        if (status.isRunning) {
            if (!status.isPaused || status.doStep)
            {
                mmu._clock.t_instr = 0;
                bool interrupted = interrupts.check();
               
                if (!interrupted)
                    cpu.step();

                timer.inc();
                ppu.step();
            }

            if (ppu.can_render || status.isPaused)
            {
                gbRender.Render();
                ppu.can_render = false;
               

            }

            status.doStep = false;
            joypad.check(mmu._clock.t_instr);
            

        }
        else
        {
            Debug.Log("System Stopped");
        }
        
       
    }
    public void Update()
    {
        Time.timeScale = frame;
        if (!auto)
            return;

        for (int i = 0; i < ticks; i++)
        {
            DoStep();
        }
       
      



    }
}
