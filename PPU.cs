using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PPU
{
    Registers registers;
    MMU mmu;
    Interrupt interrupts;
    public byte scrollX
    {
        get { return mmu.memory[0xFF43]; }
        set
        {
            mmu.memory[0xFF43] = value;
        }
    }
    public byte scrollY
    {
        get { return mmu.memory[0xFF42]; }
        set { mmu.memory[0xFF42] = value; }
    }
    public byte scanline;
    
    public int tick;

    public class Control
    {


        public byte _value;
        private byte _bgDisplay, _spriteDisplayEnable, _srpiteSize, _bgDisplaySelect, _bgWindowDataSelect, _windowEnable, _bgWindowDisplaySelect, _lcdEnable;
        public byte bgDisplay
        {
            get { return (byte)(_value & 0x01); }
            set {  _bgDisplay = value; UpdateValue(); }
        }

        public byte spriteDisplayEnable
        {
            get { return (byte)(_value & 0x02); }
            set {  _spriteDisplayEnable = value; UpdateValue(); }
        }

        public byte spriteSize
        {
            get { return (byte)(_value & 0x04); }
            set {  _srpiteSize = value; UpdateValue(); }
        }

        public byte bgDisplaySelect
        {
            get { return (byte)(_value & 0x08); }
            set { _bgDisplaySelect = value; UpdateValue(); }
        }

        public byte bgWindowDataSelect
        {
            get { return (byte)(_value & 0x10); }
            set {  _bgWindowDataSelect = value; UpdateValue(); }
        }

        public byte windowEnable
        {
            get { return (byte)(_value & 0x20); }
            set {  _windowEnable = value; UpdateValue(); }
        }

        public byte windowDisplaySelect
        {
            get { return (byte)(_value & 0x40); }
            set { _bgWindowDisplaySelect = value; UpdateValue(); }
        }

        public byte lcdEnable
        {
            get { return (byte)(_value & 0x80); }
            set { _lcdEnable = value;  UpdateValue(); }
        }


        public void UpdateControlMemory(MMU mmu)
        {
            mmu.memory[0xff41] = this._value;
        }
        public void UpdateValue()
        {
            this._value = (byte)
            (
               (bgDisplay << 0) |
               (spriteDisplayEnable << 1) |
               (spriteSize << 2) |
               (bgDisplaySelect << 3) |
               (bgWindowDataSelect << 4) |
               (windowEnable << 5) |
               (windowDisplaySelect << 6) |
               (lcdEnable << 7)
            );
        }
        public Control()
        {
            bgDisplay = 1;
            spriteDisplayEnable = 1;
            spriteSize = 1; // True means 8x16 tiles
            bgDisplaySelect = 1;
            bgWindowDataSelect = 1;
            windowEnable = 1;
            windowDisplaySelect = 1;
            lcdEnable = 1;
            _value = (byte)
            (
               (bgDisplay << 0) |
               (spriteDisplayEnable << 1) |
               (spriteSize << 2) |
               (bgDisplaySelect << 3) |
               (bgWindowDataSelect << 4) |
               (windowEnable << 5) |
               (windowDisplaySelect << 6) |
               (lcdEnable << 7)
            );

        }
    }

    public Control control;


    public class Stat
    {
        public byte _value;
        private byte _mode_flag, _coincidence_flag, _hblank, _vblank, _oam, _cinterrupt;
        public byte mode_flag
        {
            get
            {
                return (byte)(_value & 0x03);
            }
            set
            {
                _mode_flag = value;  UpdateValue();
            }

        }
        public byte coincidence_flag
        {
            get
            {
                return (byte)(_value & 0x04);
            }
            set
            {
                _coincidence_flag = value; UpdateValue();
            }
            
           
           
            
        }
        
        
        public byte hblank_interrupt
        {
            get
            {
                return (byte)(_value & 0x08);
            }
            set
            {
                _hblank = value; UpdateValue();
            }

        }
        public byte vblank_interrupt
        {
            get
            {
                return (byte)(_value & 0x10);
            }
            set
            {
                _vblank = value; UpdateValue();

            }

        }
        public byte oam_interrupt
        {
            get
            {
                return (byte)(_value & 0x20);
            }
            set
            {
                _oam = value; UpdateValue();
            }

        }
        public byte coincidence_interrupt
        {
            get
            {
                return (byte)(_value & 0x40);
            }
            set
            {
                _cinterrupt = value;  UpdateValue();
            }

        }
        public void UpdateValue()
        {
            _value = (byte)
            (
                (mode_flag & 0x03) |
                (coincidence_flag & 0x04) |
                (hblank_interrupt & 0x08) |
                (vblank_interrupt & 0x10) |
                (oam_interrupt & 0x20) |
                (coincidence_interrupt & 0x40)
            );
        }
        public Stat()
        {
            mode_flag = 2;
            coincidence_flag = 1;
            hblank_interrupt = 1;
            vblank_interrupt = 1;
            oam_interrupt = 1;
            coincidence_interrupt = 1;
            _value = (byte)
            (
                (mode_flag & 0x03) |
                (coincidence_flag & 0x04) |
                (hblank_interrupt & 0x08) |
                (vblank_interrupt & 0x10) |
                (oam_interrupt & 0x20) |
                (coincidence_interrupt & 0x40)
            );
        }
        public void UpdateStatMemory(MMU mmu)
        {
            mmu.memory[0xff41] = this._value;
        }
        
    }
    public Stat stat;
    public MColor[] framebuffer = new MColor[160 * 144];


    public byte[] background = new byte[32 * 32];

    public int mode = 0;
    public int modeclock = 0;

    public bool can_render = false;


    public PPU(Registers registers, Interrupt interrupts, MMU mmu)
    {
        for (int i = 0; i < 160 * 144; i++)
        {
            framebuffer[i] = new MColor(0,0,0,255);
        }
        for (int i = 0; i < 32 *32; i++)
        {
            background[i] = 0;
        }
        this.registers = registers;
        this.mmu = mmu;
        this.interrupts = interrupts;
        this.stat = new Stat();
        this.control = new Control();
       

        
    }

    public void step()
    {
        
        modeclock += mmu._clock.t_instr;
        stat._value = mmu.memory[0xff41];
        control._value = mmu.memory[0xFF40];
        


        if (control.lcdEnable == 0)
        {
            //Debug.Log("NO LCD");

            mode = 0;
            if (modeclock >= 70224) modeclock -= 70224;
            return;
        }
        


        switch (mode)
        {
            case 0: // HBLANK
                //Debug.Log("HBLANK");

                if (modeclock >= 204)
                {
                    modeclock -= 204;
                    mode = 2;

                    scanline += 1;
                    mmu.memory[0xff44] = scanline;

                    compare_ly_lyc();
                   

                    if (scanline == 144)
                    {
                        mode = 1;
                        can_render = true;
                        interrupts.set_interrupt_flag((int)InterruptFlags.INTERRUPT_VBLANK);
                        if (stat.vblank_interrupt != 0)
                            interrupts.set_interrupt_flag((int)InterruptFlags.INTERRUPT_LCD);
                    }
                    else if (stat.oam_interrupt != 0)
                        interrupts.set_interrupt_flag((byte)InterruptFlags.INTERRUPT_LCD);

                    mmu.write_byte(0xff41, (byte)((mmu.read_byte(0xff41) & 0xFC) | (mode & 3)));
                   



                }
                break;
            case 1: // VBLANK
                //Debug.Log("VBLANK");


                if (modeclock >= 456)
                {
                    modeclock -= 456;
                    scanline += 1;
                    mmu.memory[0xff44] = scanline;

                    compare_ly_lyc();
                    if (scanline == 153)
                    {
                        scanline = 0;
                        mmu.memory[0xff44] = scanline;

                        mode = 2;
                        

                        mmu.write_byte(0xff41, (byte)((mmu.read_byte(0xff41) & 0xFC) | (mode & 3)));
                        

                        if (stat.oam_interrupt != 0)
                            interrupts.set_interrupt_flag((byte)InterruptFlags.INTERRUPT_LCD);
                    }
                }

                break;
            case 2: // OAM

                //Debug.Log("OAM");

                if (modeclock >= 80)
                {
                    modeclock -= 80;
                    mode = 3;
                    

                    mmu.write_byte(0xff41, (byte)((mmu.read_byte(0xff41) & 0xFC) | (mode & 3)));
                  

                }
                break;
            case 3: // VRAM

                //Debug.Log("VRAM");
                if (modeclock >= 172)
                {
                    modeclock -= 172;
                    mode = 0;
                    render_scan_lines();
                    mmu.write_byte(0xff41, (byte)((mmu.read_byte(0xff41) & 0xFC) | (mode & 3)));
                   

                    if (stat.hblank_interrupt != 0)
                        interrupts.set_interrupt_flag((byte)InterruptFlags.INTERRUPT_LCD);
                }
                break;
        }
        
    }

    
    private void compare_ly_lyc()
    {
        compare_ly_lyc(stat);
    }

    void compare_ly_lyc(Stat stat)
    {
        byte lyc = mmu.read_byte(0xFF45);
        stat.coincidence_flag = (byte)(lyc == scanline ? 1 : 0);


        if (lyc == scanline && stat.coincidence_interrupt != 0)
            this.interrupts.set_interrupt_flag((byte)InterruptFlags.INTERRUPT_LCD);
    }

    void render_scan_lines()
    {
        bool[] row_pixels = new bool[160];
        this.render_scan_line_background(ref row_pixels);
        this.render_scan_line_window();
        this.render_scan_line_sprites(ref row_pixels);
    }
    void render_scan_line_background(ref bool[] row_pixels)
    {
       
        ushort address = 0x9800;
        if (this.control.bgDisplaySelect != 0)
            address += 0x400;

        address += (ushort)(((this.scrollY + this.scanline) / 8 * 32) % (32 * 32));

        ushort start_row_address = address;
        ushort end_row_address = (ushort)(address + 32);
        address += (ushort)(this.scrollX >> 3);

        int x = this.scrollX & 7;
        int y = (this.scanline + this.scrollY) & 7;
        int pixelOffset = this.scanline * 160;
        int pixel = 0;
        for (int i = 0; i < 21; i++)
        {
            ushort tile_address = (ushort)(address + i);
            if (tile_address >= end_row_address)
                tile_address = (ushort)(start_row_address + tile_address % end_row_address);

            int tile = this.mmu.read_byte(tile_address);
            if (this.control.bgWindowDataSelect == 0 && tile < 128)
                tile += 256;

            for (; x < 8; x++)
            {
                if (pixel >= 160) break;
                if (this.scanline < 144)
                {
                    int colour = mmu.tiles[tile].pixels[y, x];
                    framebuffer[pixelOffset++] = mmu.palette_BGP[colour];
                    if (colour > 0)
                        row_pixels[pixel] = true;
                    pixel++;
                }
                

            }
            x = 0;
        }
    }


    void render_scan_line_window()
    {
      

        if (this.control.windowEnable == 0)
        {
            return;
        }

        if (mmu.read_byte(0xFF4A) > this.scanline)
        {
            return;
        }

        ushort address = 0x9800;
        if (this.control.windowDisplaySelect != 0)
            address += 0x400;

        address += (ushort)(((this.scanline - mmu.read_byte(0xFF4A)) / 8) * 32);
        int y = (this.scanline - mmu.read_byte(0xFF4A)) & 7;
        int x = 0;

        int pixelOffset = this.scanline * 160;
        pixelOffset += mmu.read_byte(0xFF4B) - 7;
        for (ushort tile_address = address; tile_address < address + 20; tile_address++)
        {
            int tile = this.mmu.read_byte(tile_address);

            if (this.control.bgWindowDataSelect == 0  && tile < 128)
                tile += 256;

            for (; x < 8; x++)
            {
                if (pixelOffset > framebuffer.Length) continue;
                int colour = mmu.tiles[tile].pixels[y,x];
                framebuffer[pixelOffset++] = mmu.palette_BGP[colour];
            }
            x = 0;
        }

    }

    void render_scan_line_sprites(ref bool[] row_pixels)
    {


        int sprite_height = control.spriteSize != 0 ? 16 : 8;

        bool[] visible_sprites = new bool[40];
        int sprite_row_count = 0;

        for (int i = 39; i >= 0; i--)
        {
           

            if (!mmu._sprites[i].ready)
            {
                visible_sprites[i] = false;
                continue;
            }

            if ((mmu._sprites[i].y > scanline) || ((mmu._sprites[i].y + sprite_height) <= scanline))
            {
                visible_sprites[i] = false;
                continue;
            }

            visible_sprites[i] = sprite_row_count++ <= 10;
        }

        for (int i = 39; i >= 0; i--)
        {
            if (!visible_sprites[i])
                continue;

            MMU.Sprite sprite = mmu._sprites[i];

            if ((sprite.x < -7) || (sprite.x >= 160))
                continue;

            // Flip vertically
            int pixel_y = scanline - mmu._sprites[i].y;
            pixel_y = mmu._sprites[i].yFlip != 0 ? (7 + 8 * control.spriteSize) - pixel_y : pixel_y;

            for (int x = 0; x < 8; x++)
            {
                int tile_num = mmu._sprites[i].tile & (control.spriteSize != 0 ? 0xFE : 0xFF);
                int colour = 0;

                int x_temp = mmu._sprites[i].x + x;
                if (x_temp < 0 || x_temp >= 160)
                    continue;

                int pixelOffset = this.scanline * 160 + x_temp;

                // Flip horizontally
                byte pixel_x = mmu._sprites[i].xFlip != 0 ? (byte)(7 - x ): (byte)x;

                if (control.spriteSize != 0 && (pixel_y >= 8))
                    colour = mmu.tiles[tile_num + 1].pixels[pixel_y - 8,pixel_x];
                else
                    colour = mmu.tiles[tile_num].pixels[pixel_y,pixel_x];

                // Black is transparent
                if (colour == 0)
                    continue;

                if (!row_pixels[x_temp] || mmu._sprites[i].renderPriority == 0)
                    framebuffer[pixelOffset] = mmu._sprites[i].colourPalette[colour];
            }
        }
    }

}
