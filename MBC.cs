using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MBC
{

    public int rom_banks_count = 1;
    public int ram_banks_count = 1;
    public Cartridge cart;

    public virtual byte read_byte(ushort address)
    {
       
        return 0;
    }
    public virtual void write_byte(ushort address, byte value)
    {
        
    }
    public void mbc_MBC(Cartridge cartAdd) 
    { 
        this.cart = cartAdd; 
    }
    public void mbc_MBC(Cartridge cartAdd, int rom_banks_count, int ram_banks_count) 
    {
        this.cart = cartAdd;
        this.rom_banks_count = rom_banks_count;
        this.ram_banks_count = ram_banks_count;
    }
}
class MBC0 :  MBC
{


    public override byte read_byte(ushort address)
    {
        if ((ushort)address < 0x8000)
            return this.cart.ram[address];

        return 0;
    }
}
class MBC1 : MBC
{
    public bool ram_enabled = false;
    public bool mode = false;
    public byte rom_bank = 1;
    public byte ram_bank = 0;

    public override byte read_byte(ushort address)
    {
        //Debug.Log("MBC Read Address: 0x" + address.ToString("X2"));
        if (address < 0x4000)
        {
            int bank = (mode ? 1: 0) * (ram_bank << 5) % rom_banks_count;
            return cart.memory[bank * 0x4000 + address];
        }
        else if (address < 0x8000)
        {
            int bank = ((ram_bank << 5) | rom_bank) % rom_banks_count;
            return cart.memory[bank * 0x4000 + address - 0x4000];
        }
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
            {
                int bank = (mode ? 1 : 0) * ram_bank % ram_banks_count;
                return cart.ram[bank * 0x2000 + address - 0xA000];
            }
        }
        return 0xFF;
    }

    public override void write_byte(ushort address, byte value)
    {

        //Debug.Log("MBC Write Address: 0x" + address.ToString("X2"));
       

        if (address < 0x2000)
        {
            ram_enabled = (value & 0x0F) == 0x0A;
        }
        else if (address < 0x4000)
        {
            value &= 0x1F;
            if (value == 0)
                value = 1;
            rom_bank = (byte)value;
        }
        else if (address < 0x6000)
        {
            ram_bank = (byte)((byte)value & 0x3);
        }
        else if (address < 0x8000)
        {
            mode = (value & 0x01) != 0;
        }
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
            {
                int bank = (ram_bank * (mode ? 1 : 0)) % ram_banks_count;
                cart.ram[bank * 0x2000 + address - 0xA000] = (byte)value;
            }
        }
    }
}

class MBC2 : MBC1
{
    public override byte read_byte(ushort address)
    {
        if (address < 0x4000)
            return cart.memory[address];
        else if (address < 0x8000)
            return cart.memory[rom_bank * 0x4000 + address - 0x4000];
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
                return cart.ram[ram_bank * 0x2000 + address - 0xA000];
        }

        return 0;
    }
    public override void write_byte(ushort address, byte value)
    {
        if (address < 0x2000)
        {
            if ((address & 0x0100) == 0)
                ram_enabled = value == 0x0a;
        }
        else if (address < 0x400)
        {
            if ((address & 0x0100) != 0)
                rom_bank = (byte)value;
        }
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
                cart.ram[ram_bank * 0x2000 + address - 0xA000] = (byte)value;
        }
    }
}
class MBC3 : MBC1
{
    public override byte read_byte(ushort address)
    {
        if (address < 0x4000)
            return cart.memory[address];
        else if (address < 0x8000)
            return cart.memory[rom_bank * 0x4000 + address - 0x4000];
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
            {
                if (ram_bank <= 0x03)
                    return cart.ram[ram_bank * 0x2000 + address - 0xA000];
            }
        }

        return 0;
    }
    public override void write_byte(ushort address, byte value)
    {
        if (address < 0x2000)
            ram_enabled = (value & 0x0f) == 0x0a;
        else if (address < 0x4000)
        {
            rom_bank = (byte)((byte)value & 0x7f);
            if (rom_bank == 0x00)
                rom_bank = 0x01;
        }
        else if (address < 0x6000)
            ram_bank = (byte)(value & 0x0f);
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
            {
                if (ram_bank <= 0x03)
                    cart.ram[ram_bank * 0x2000 + address - 0xA000] = (byte)value;
            }
        }
    }


}
class MBC5 : MBC1
{
    public override byte read_byte(ushort address)
    {
        if (address < 0x4000)
            return cart.memory[address];
        else if (address < 0x8000)
            return cart.memory[rom_bank * 0x4000 + address - 0x4000];
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
                return cart.ram[ram_bank * 0x2000 + address - 0xA000];
        }

        return 0;
    }
    public override void write_byte(ushort address, byte value)
    {
        if (address < 0x2000)
            ram_enabled = (value & 0x0f) == 0x0a;
        else if (address < 0x3000)
            rom_bank = (byte)((rom_bank & 0x100) | value);
        else if (address < 0x4000)
            rom_bank = (byte)((byte)(rom_bank & 0xff) | ((value & 0x01) << 8));
        else if (address < 0x6000)
            ram_bank = (byte)((value & 0x0f) % ram_banks_count);
        else if (address >= 0xA000 && address < 0xC000)
        {
            if (ram_enabled)
                cart.ram[ram_bank * 0x2000 + address - 0xA000] = (byte)value;
        }
    }
}
