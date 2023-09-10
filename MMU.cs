using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;



public class Color
{
    public byte[] colors; // Array to store color components (r, g, b, a)

    // Properties for color components
    public byte r
    {
        get { return colors[0]; }
        set { colors[0] = value; }
    }

    public byte g
    {
        get { return colors[1]; }
        set { colors[1] = value; }
    }

    public byte b
    {
        get { return colors[2]; }
        set { colors[2] = value; }
    }

    public byte a
    {
        get { return colors[3]; }
        set { colors[3] = value; }
    }

    // Constructor to initialize the color components
    public Color(byte r, byte g, byte b, byte a)
    {
        colors = new byte[4];
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
}
public class MMU
{
    public byte[] noobboy_boot_rom = new byte[256] {
            0x31, 0xfe, 0xff, 0xaf, 0x21, 0xff, 0x9f, 0x32, 0xcb, 0x7c, 0x20, 0xfb,
            0x21, 0x26, 0xff, 0x0e, 0x11, 0x3e, 0x80, 0x32, 0xe2, 0x0c, 0x3e, 0xf3,
            0xe2, 0x32, 0x3e, 0x77, 0x77, 0x3e, 0xfc, 0xe0, 0x47, 0x11, 0xa8, 0x00,
            0x21, 0x10, 0x80, 0x1a, 0xcd, 0x95, 0x00, 0xcd, 0x96, 0x00, 0x13, 0x7b,
            0xfe, 0x34, 0x20, 0xf3, 0x11, 0xd8, 0x00, 0x06, 0x08, 0x1a, 0x13, 0x22,
            0x23, 0x05, 0x20, 0xf9, 0x3e, 0x19, 0xea, 0x10, 0x99, 0x21, 0x2f, 0x99,
            0x0e, 0x0c, 0x3d, 0x28, 0x08, 0x32, 0x0d, 0x20, 0xf9, 0x2e, 0x0f, 0x18,
            0xf3, 0x67, 0x3e, 0x64, 0x57, 0xe0, 0x42, 0x3e, 0x91, 0xe0, 0x40, 0x04,
            0x1e, 0x02, 0x0e, 0x0c, 0xf0, 0x44, 0xfe, 0x90, 0x20, 0xfa, 0x0d, 0x20,
            0xf7, 0x1d, 0x20, 0xf2, 0x0e, 0x13, 0x24, 0x7c, 0x1e, 0x83, 0xfe, 0x62,
            0x28, 0x06, 0x1e, 0xc1, 0xfe, 0x64, 0x20, 0x06, 0x7b, 0xe2, 0x0c, 0x3e,
            0x87, 0xe2, 0xf0, 0x42, 0x90, 0xe0, 0x42, 0x15, 0x20, 0xd2, 0x05, 0x20,
            0x4f, 0x16, 0x20, 0x18, 0xcb, 0x4f, 0x06, 0x04, 0xc5, 0xcb, 0x11, 0x17,
            0xc1, 0xcb, 0x11, 0x17, 0x05, 0x20, 0xf5, 0x22, 0x23, 0x22, 0x23, 0xc9,
            0xce, 0xed, 0x66, 0x66, 0x00, 0x07, 0x00, 0x08, 0x00, 0x0f, 0x33, 0x33,
            0x00, 0x0e, 0x76, 0x67, 0xc6, 0x6c, 0x00, 0x07, 0x00, 0x09, 0x00, 0x0b,
            0xdc, 0xcc, 0x6e, 0xe6, 0xcc, 0xc7, 0xdd, 0xd8, 0x99, 0x9f, 0xbb, 0xb3,
            0x33, 0x3e, 0x66, 0x67, 0x66, 0x6c, 0xcc, 0xc7, 0xdc, 0xc8, 0xbf, 0x6c,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0xa8, 0x00, 0x11,
            0xa8, 0x00, 0x1a, 0x13, 0xbe, 0x20, 0xfe, 0x23, 0x7d, 0xfe, 0x34, 0x00,
            0x00, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xfb, 0x86, 0x00, 0x00,
            0x3e, 0x01, 0xe0, 0x50
        };

    public byte[] memory = new byte[0xFFFF + 1];
    public Cartridge cartridge;

    public byte joypad = 0xFF;

    public bool cgb_mode = false;

    public Registers register;
    public class clock
    {
        public int t = 0;
        public int t_instr = 0;
    }
    public clock _clock;

    public class timer
    {
        public ushort div = 0;
        public byte tima = 0;
        public byte tma = 0;
        public byte tac = 0;
    }
    public timer _timer;

  

    public class Sprite
    {
        public bool ready;
        public int y;
        public int x;
        public int tile;
        public Color[] colourPalette;

        public byte value = 0xFF;
        public byte gbcPaletteNumber1 => (byte)((value >> 0) & 0x01);
        public byte gbcPaletteNumber2 => (byte)((value >> 1) & 0x01);
        public byte gbcPaletteNumber3 => (byte)((value >> 2) & 0x01);
        public byte gbcVRAMBank => (byte)((value >> 3) & 0x01);
        public byte paletteNumber => (byte)((value >> 4) & 0x01);
        public byte xFlip => (byte)((value >> 5) & 0x01);
        public byte yFlip => (byte)((value >> 6) & 0x01);
        public byte renderPriority => (byte)((value >> 7) & 0x01);
        public void UpdateSprite(byte value)
        {
            this.value = value;
            
    }

    }

    public Sprite[] _sprites = new Sprite[40];
    //create init

    public class Tile
    {
        public byte[][] pixels = new byte[8][]
        {
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },



        };
    }
    public Tile[] tiles = new Tile[384];

    public  Color[] palette_colours = new Color[4];
   
    public  Color[] palette_BGP = new Color[4];
   
    public  Color[] palette_OBP0 = new Color[4];
   

    public  Color[] palette_OBP1 = new Color[4];
    


    public bool romDisabled = false;
    public bool is_halted = false;
    public bool trigger_halt_bug = false;

    public void init()
    {
        _timer = new timer();
        _clock = new clock();

        for (int x = 0; x < 40; x++)
        {
            _sprites[x] = new Sprite();
        }

          for (int g = 0; g < 4; g++)
          {

            if (g == 0)
              {
                    palette_colours[g] = new Color(255, 255, 255, 255);
                    palette_BGP[g] = new Color(255, 255, 255, 255);
                    palette_OBP0[g] = new Color(0, 0, 0, 255);
                    palette_OBP1[g] = new Color(0, 0, 0, 255);
                    
              }
              else if (g == 1)
              {
                palette_colours[g] = new Color(192, 192, 192, 255);
                palette_BGP[g] = new Color(0, 0, 0, 255);
                palette_OBP0[g] = new Color(0, 0, 0, 255);
                palette_OBP1[g] = new Color(0, 0, 0, 255);
                
              }
              else if (g == 2)
              {
                    palette_colours[g] = new Color(96, 96, 96, 255);
                    palette_BGP[g] = new Color(0, 0, 0, 255);
                    palette_OBP0[g] = new Color(0, 0, 0, 255);
                    palette_OBP1[g] = new Color(0, 0, 0, 255);

              }
              else if (g == 3)
              {
                    palette_colours[g] = new Color(0, 0, 0, 255);
                    palette_BGP[g] = new Color(0, 0, 0, 255);
                    palette_OBP0[g] = new Color(0, 0, 0, 255);
                    palette_OBP1[g] = new Color(0, 0, 0, 255);
               
              }


          }
    }

    public MMU(Cartridge cart, Registers registers)
    {
       
        this.cartridge = cart;
        this.register = registers;
        memory = new byte[0xFFFF + 1];
        for (int i = 0; i < 384; i++)
        {
            tiles[i] = new Tile();
        }
        init();

    }

    public void load_boot_rom(string location)
    {
        try
        {
            using (BinaryReader gameRom = new BinaryReader(File.Open(location, FileMode.Open)))
            {
                long size = gameRom.BaseStream.Length;
                
                gameRom.Read(memory, 0x0, 256);

            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading game ROM: " + e.Message);
        }
    }

    public void load_default_boot_rom()
    {
        
        Array.Copy(noobboy_boot_rom, memory, noobboy_boot_rom.Length);
            
    }

    public void save_game_state()
    {
        cartridge.write_save_state();
    }
    public void UpdateTile(ushort laddress, byte value)
    {
       

        ushort address = (ushort)(laddress & 0xFFFE);

        ushort tile = (ushort)((address >> 4) & 511);
        ushort y = (ushort)((address >> 1) & 7);

        byte bitIndex;
        for (ushort x = 0; x < 8; x++)
        {
            bitIndex = (byte)(1 << (7 - x));

            tiles[tile].pixels[y][x] = (byte)(((memory[address] & bitIndex) != 0 ? 1 : 0) + ((memory[address + 1] & bitIndex) != 0 ? 2 : 0));
           
        }
    }

    public void UpdateSprite(ushort laddress, byte value)
    {
        ushort address = (ushort)(laddress - 0xFE00);
        _sprites[address >> 2].ready = false;
        switch (address & 3)
        {
            case 0:
                _sprites[address >> 2].y = value - 16;
                break;
            case 1:
                _sprites[address >> 2].x = value - 8;
                break;
            case 2:
                _sprites[address >> 2].tile = value;
                break;
            case 3:
                _sprites[address >> 2].UpdateSprite(value);
                //sprite.value = value;
                _sprites[address >> 2].colourPalette = (_sprites[address >> 2].paletteNumber != 0) ? palette_OBP1 : palette_OBP0;
                _sprites[address >> 2].ready = true;
                break;
        }
    }
    public void UpdatePallete(ref Color[] pallete, byte value)
    {
        
        pallete[0] = palette_colours[value & 0x3];
        pallete[1] = palette_colours[(value >> 2) & 0x3];
        pallete[2] = palette_colours[(value >> 4) & 0x3];
        pallete[3] = palette_colours[(value >> 6) & 0x3];
       

    }

    public byte read_byte(ushort address)
    {
        //Debug.Log("MMU Read Address: 0x" + address.ToString("X2"));

        if (address == 0xff00)
        {
            switch (memory[0xff00] & 0x30)
            { // Mask `00110000` to check which SELECT
                case 0x10: return (byte)((joypad & 0x0F) | 0x10);
                case 0x20: return (byte)((joypad >> 4) & 0x0F | 0x20);
                default: return 0xFF;
            }
        }

        //Timers
        else if (address == 0xff04) return (byte)_timer.div;
        else if (address == 0xff05) return _timer.tima;
        else if (address == 0xff06) return _timer.tma;
        else if (address == 0xff07) return _timer.tac;

        if (address == 0xff0f) return memory[0xFF0F];

        if (address < 0x100 && !romDisabled) return memory[address];

        // Switchable ROM banks
        if (address < 0x8000)
            return cartridge.mbc_read(address);

        // Switchable RAM banks
        if (address >= 0xA000 && address <= 0xBFFF)
            return cartridge.mbc_read(address);

        return memory[address];
    }
    public void write_byte(ushort address, byte value)
    {
       

        if (address == 0xFF40)
        {
            
            memory[address] = value;//update LCD Control
           
            if ((value & (1 << 7)) == 0)
            {
                memory[0xFF44] = 0x00;
                memory[0xFF41] &= 0x7C;
                

            }
        }

        if (address >= 0xFEA0 && address <= 0xFEFF) // Writing in unused area
            return;

        // Copy Sprites from ROM to RAM (OAM)
        if (address == 0xFF46)
            for (ushort i = 0; i < 160; i++) write_byte((ushort)(0xFE00 + i), read_byte((ushort)((value << 8) + i)));


        if (address == 0xff50)
            romDisabled = true;

        //Timers
        else if (address == 0xff04) _timer.div = 0;
        else if (address == 0xff05) _timer.tima = value;
        else if (address == 0xff06) _timer.tma = value;
        else if (address == 0xff07) _timer.tac = value;

        // Update colour palette
        else if (address == 0xff47) UpdatePallete(ref palette_BGP, value);
        else if (address == 0xff48) UpdatePallete(ref palette_OBP0,value);
        else if (address == 0xff49) UpdatePallete(ref palette_OBP1, value);

        // Switchable ROM banks
        if (address < 0x8000)
            cartridge.mbc_write(address, value);
        else if (address >= 0xA000 && address < 0xC000)
            cartridge.mbc_write(address, value);
        else
            memory[address] =value;


        if (address >= 0x8000 && address < 0x9800)
            UpdateTile(address, value);

        if (address >= 0xFE00 && address <= 0xFE9F)
            UpdateSprite(address, value);


    }


    public void write_short(ushort address, ushort value)
    {
        write_byte((ushort)address, (byte)(value & 0x00ff));
        write_byte((ushort)(address + 1), (byte)((value & 0xff00) >> 8));
    }
    public void write_short_stack(ref ushort sp, ushort value)
    {
        sp -= 2;
        write_short(sp, value);
    }
    public ushort read_short(ushort address)
    {
        return (ushort)(read_byte(address) | (read_byte((ushort)(address + 1)) << 8));
    }
    public ushort read_short_stack(ref ushort sp)
    {
        ushort value = read_short(sp);
        sp += 2;
        //Debug.Log("Mem Read: 0x" + value.ToString("X2"));
        return value;
    }
}
