using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class Cartridge
{
    public byte[] rom_title = new byte[16];
    public bool cgb_game = false;

    public MBC mbc;

    public byte[] memory;
    public byte[] ram;

    int rom_banks_count;
    int ram_banks_count;

    public void _Cartridge(string rom, string save_file)
    {
        load_game_rom(rom);
        load_save_state(save_file);
    }
    public void load_save_state(string save_file)
    {

        if (string.IsNullOrEmpty(save_file))
            return;

        try
        {
            using (BinaryReader saveStream = new BinaryReader(File.Open(save_file, FileMode.Open)))
            {
                long size = saveStream.BaseStream.Length;
                if (size != (0x7F * 0x2000 + rom_title.Length))
                {
                    Debug.Log("Save file possibly corrupted. Save not loaded.");
                    return;
                }

                byte[] saveTitleBytes = saveStream.ReadBytes(16);
                string saveTitle = System.Text.Encoding.ASCII.GetString(saveTitleBytes);
                Debug.Log("Save file " + saveTitle);

                if (!string.Equals(rom_title, saveTitle))
                {
                    Debug.Log("This save file is not for this ROM. Save not loaded.");
                    return;
                }

                saveStream.BaseStream.Seek(rom_title.Length, SeekOrigin.Current);

                byte[] ramData = saveStream.ReadBytes((int)(0x7F * 0x2000));
                Buffer.BlockCopy(ramData, 0, ram, 0, ramData.Length);

                Debug.Log("Save file loaded successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading save file: " + e.Message);
        }
    }
    public void load_game_rom(string location)
    {
        try
        {
            using (BinaryReader gameRom = new BinaryReader(File.Open(location, FileMode.Open)))
            {
                long size = gameRom.BaseStream.Length;
                if (size % (16 * 1024) != 0)
                {
                    Debug.Log("Size must be a multiple of 16 KB");
                    return;
                }

                memory = new byte[size];
                gameRom.Read(memory, 0x0, (int)size);
                rom_banks_count = (int)(size / 0x4000);
                ram_banks_count = get_ram_banks_count(memory[0x149]);

                ram = new byte[ram_banks_count * 0x2000];

                Buffer.BlockCopy(memory, 0x134, rom_title, 0x0, 16);
                cgb_game = (memory[0x143] == 0x80 || memory[0x143] == 0xC0);
                detect_mbc_type(memory[0x147]);
                string title = "";
                for (int i = 0; i < rom_title.Length; i++)
                {
                    title += (char)rom_title[i];
                }
                Debug.Log("Rom Title : " + title);
                Debug.Log("CGB Game : " + cgb_game);
                Debug.Log("MBC: " + memory[0x147]);
                Debug.Log(memory[0x147].ToString("X2"));
                Debug.Log("ROM banks: " + rom_banks_count);
                Debug.Log("RAM banks: " + ram_banks_count);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading game ROM: " + e.Message);
        }
    }

    public void detect_mbc_type(byte type)
    {
        
        switch (type)
        {
           
            case 0x00:
            case 0x08:
            case 0x09:
                mbc = new MBC0();
                mbc.mbc_MBC(this);
                break;
            case 0x01:
            case 0x02:
            case 0x03:
                mbc = new MBC1();
                mbc.mbc_MBC(this, rom_banks_count, ram_banks_count);
                break;
            case 0x05:
            case 0x06:
                mbc = new MBC2();
                mbc.mbc_MBC(this, rom_banks_count, ram_banks_count);
                break;
            case 0x0F:
            case 0x10:
            case 0x11:
            case 0x12:
            case 0x13:
                mbc = new MBC3();
                mbc.mbc_MBC(this, rom_banks_count, ram_banks_count);
                break;
            case 0x19:
            case 0x1A:
            case 0x1B:
            case 0x1C:
            case 0x1D:
            case 0x1E:
                mbc = new MBC5();
                mbc.mbc_MBC(this, rom_banks_count, ram_banks_count);
                break;
            default:
                Debug.Log("Unsupported MBC type: " + type);
                break;
        }
    }

    public int get_ram_banks_count(byte type)
    {
        switch (type)
        {
            case 0x00: return 0;
            case 0x01: return 0;
            case 0x02: return 1;
            case 0x03: return 4;
            case 0x04: return 16;
            case 0x05: return 8;
            default:
                Debug.Log("Incorrect RAM type: " + type);
                return 12;
        }
    }

    public void write_save_state()
    {
        string saveDirectory = "saves";
        Directory.CreateDirectory(saveDirectory);

        DateTime currentTime = DateTime.Now;
        string timestamp = currentTime.ToString("dd-MM-yyyy HH-mm-ss");

        string romTitleString = System.Text.Encoding.ASCII.GetString(rom_title);
        string saveFileName = $"{saveDirectory}/{romTitleString}_{timestamp}.save";

        using (FileStream fileStream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
        {
            // Write rom_title bytes to the save file
            fileStream.Write(rom_title, 0, rom_title.Length);

            // Write ram bytes to the save file
            byte[] ramBytes = new byte[0x7F * 0x2000];
            // Fill 'ramBytes' with your RAM data here

            fileStream.Write(ramBytes, 0, ramBytes.Length);
        }

        Debug.Log("Saved state to: " + saveFileName);
    }

    public byte mbc_read(ushort address) 
    {
        //Debug.Log("MBC Read @ 0x" + address.ToString("X2"));

        return mbc.read_byte(address);

    }
    public void mbc_write(ushort address, byte value) 
    {
        mbc.write_byte(address, value);

    }
}
