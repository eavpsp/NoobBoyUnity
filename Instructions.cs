using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Instructions
{
    public Registers registers;
    public MMU mmu;
    public Interrupt interrupts;
    byte tempBytes = new byte();
    private readonly byte[] instructionTicks = {
            4, 12, 8, 8, 4, 4, 8, 4, 20, 8, 8, 8, 4, 4, 8, 4,       // 0x0_
            4, 12, 8, 8, 4, 4, 8, 4,  12, 8, 8, 8, 4, 4, 8, 4,      // 0x1_
            0, 12, 8, 8, 4, 4, 8, 4,  0, 8, 8, 8, 4, 4, 8, 4,       // 0x2_
            0, 12, 8, 8, 12, 12, 12, 4,  0, 8, 8, 8, 4, 4, 8, 4,    // 0x3_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x4_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x5_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x6_
            8, 8, 8, 8, 8, 8, 4, 8,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x7_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x8_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0x9_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0xa_
            4, 4, 4, 4, 4, 4, 8, 4,  4, 4, 4, 4, 4, 4, 8, 4,        // 0xb_
            0, 12, 0, 16, 0, 16, 8, 16,  0, 16, 0, 0, 0, 24, 8, 16, // 0xc_
            0, 12, 0, 0, 0, 16, 8, 16,  0, 16, 0, 0, 0, 0, 8, 16,   // 0xd_
            12, 12, 8, 0, 0, 16, 8, 16,  16, 4, 16, 0, 0, 0, 8, 16, // 0xe_
            12, 12, 8, 4, 0, 16, 8, 16,  12, 8, 16, 4, 0, 0, 8, 16  // 0xf_
        };
    private readonly byte[] extendedInstructionTicks = {
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x0_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x1_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x2_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x3_
            8, 8, 8, 8, 8,  8, 12, 8,  8, 8, 8, 8, 8, 8, 12, 8,     // 0x4_
            8, 8, 8, 8, 8,  8, 12, 8,  8, 8, 8, 8, 8, 8, 12, 8,     // 0x5_
            8, 8, 8, 8, 8,  8, 12, 8,  8, 8, 8, 8, 8, 8, 12, 8,     // 0x6_
            8, 8, 8, 8, 8,  8, 12, 8,  8, 8, 8, 8, 8, 8, 12, 8,     // 0x7_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x8_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0x9_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0xa_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0xb_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0xc_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0xd_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8,     // 0xe_
            8, 8, 8, 8, 8,  8, 16, 8,  8, 8, 8, 8, 8, 8, 16, 8      // 0xf_
        };
    public GameObject debugLabel;


    public Instructions(Registers registers, Interrupt interrupts, MMU mmu)
    {
        this.registers = registers;
        this.mmu = mmu;
        this.interrupts = interrupts;
    }
    byte inc(byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, (value & 0x0f) == 0x0f);
        value += 1;
        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);
        return 1;
    }

    void dec(byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, (value & 0x0f) != 0);

        value -= 1;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, true);
    }

    void rlc(ref byte value)
    {
        byte carry = (byte)((value >> 7) & 0x01);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & (1 << 7)) != 0);

        value <<= 1;
        value += carry;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }
    void add(ref ushort destination,  ushort value)
    {
        ushort result = (ushort)(destination + value);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, result > 0xff);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((destination & 0x0f) + (value & 0x0f)) > 0x0f);

        destination = result;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, destination == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);
    }
    void add(ref byte destination,  byte value)
    {
        ushort result = (ushort)(destination + value);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, result > 0xffff);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((destination & 0x0fff) + (value & 0x0fff)) > 0x0fff);

        destination = (byte)result;

        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);
    }
    void add(ref ushort destination, byte value)
    {
        ushort result = (ushort)(destination + value);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, ((registers.sp ^ value ^ (result & 0xFFFF)) & 0x100) == 0x100);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((registers.sp ^ value ^ (result & 0xFFFF)) & 0x10) == 0x10);

        destination = (ushort)(result & 0xFFFF);

        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_ZERO, false);
    }

    void call(bool condition)
    {
        ushort operand = mmu.read_short(registers.pc);
        registers.pc += 2;

        mmu._clock.t_instr += 12;
        if (condition)
        {
            mmu.write_short_stack(ref registers.sp, registers.pc);
            registers.pc = operand;
            mmu._clock.t_instr += 12;
        }
    }

    void ret(bool condition)
    {
        if (condition)
        {
            registers.pc = mmu.read_short_stack(ref registers.sp);
            mmu._clock.t_instr += 20;
        }
        else
        {
            mmu._clock.t_instr += 8;
        }
    }

    void jump_add(bool condition)
    {
        if (condition)
        {
            sbyte offset = (sbyte)mmu.read_byte(registers.pc); // Read the signed byte value
            registers.pc = (ushort)(registers.pc + 1 + offset); // Increment PC by the signed offset
            mmu._clock.t_instr += 12;
 
        }
        else
        {
            registers.pc++;
            mmu._clock.t_instr += 8;
            

        }
    }

    void jump(bool condition)
    {
        if (condition)
        {
            registers.pc = mmu.read_short(registers.pc);
            mmu._clock.t_instr += 16;
        }
        else
        {
            registers.pc += 2;
            mmu._clock.t_instr += 12;
        }
    }

    void cp(byte value)
    {
      
        byte temp_val = registers.AF.a;
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, value > temp_val);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, (value & 0x0f) > (temp_val & 0x0f));

        temp_val -= value;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, temp_val == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, true);
    }

    void ldhl(sbyte value)
    {
        ushort result = (ushort)(registers.sp + (sbyte)value);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, ((registers.sp ^ (sbyte)value ^ result) & 0x100) == 0x100);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((registers.sp ^ (sbyte)value ^ result) & 0x10) == 0x10);
        
        registers.HL.hl = result;

        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_ZERO, false);
    }

    void adc(byte value)
    {
        int carry = registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY) ? 1 : 0;
        int result = registers.AF.a + value + carry;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, (sbyte)result == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, result > 0xff);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((registers.AF.a & 0x0F) + (value & 0x0f) + carry) > 0x0F);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);

        registers.AF.a = (byte)((sbyte)result & 0xff);
    }

    void and_(byte value)
    {
        registers.AF.a = (byte)(registers.AF.a & value);

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, true);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_CARRY, false);
    }

    void or_(byte value)
    {
        registers.AF.a |= (byte)value;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY | (byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }
    void sub(byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, value > registers.AF.a);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, (value & 0x0f) > (registers.AF.a & 0x0f));

        registers.AF.a -= value;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, true);
    }
    void bit(byte bit, byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, (value & bit) == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, true);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);
    }
    void res(byte bit, ref byte rgst)
    {
        rgst &= (byte)~(1 << bit);
    }
    void set(byte bit, ref byte rgst)
    {
        rgst |= bit;
    }
    void sbc(byte value)
    {
        bool is_carry = registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value + (is_carry ? 1 : 0)) > registers.AF.a);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, ((value & 0x0f) + (is_carry ? 1 : 0)) > (registers.AF.a & 0x0f));

        registers.AF.a -= (byte)(value + (is_carry ? 1 : 0));

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, true);
    }
    void xor_(byte value)
    {
        registers.AF.a ^= value;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY | (byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }
    void cp_n(byte operand)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, true);
        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == operand);
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, operand > registers.AF.a);
        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, (operand & 0x0f) > (registers.AF.a & 0x0f));
    }
    void rl(ref byte value)
    {
        byte carry = (byte)(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY) ? 1: 0);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & (1 << 7)) != 0);

        value <<= 1;
        value += carry;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }

    void rr(ref byte value)
    {
        byte carry = (byte)(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY) ? 1 : 0);

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & 0x01) != 0);

        value >>= 1;
        value |= (byte)(carry << 7);

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }

    void rrc(ref byte value)
    {
        int carry = value & 0x01;

        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, carry == 0);

        value >>= 1;
        value |= (byte)(carry << 7);

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }

    void sla(ref byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & (1 << 7)) != 0);

        value <<= 1;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }

    void sra(ref byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & 0x01) != 0);

        int msb = value & (1 << 7);
        value >>= 1;
        value |= (byte)msb;


        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }

    void srl(ref byte value)
    {
        registers.set_flags((byte)RegisterFlags.FLAG_CARRY, (value & 0x01) != 0 ? true: false);

        value >>= 1;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
    }


    void swap(ref byte value)
    {
        byte lower = (byte)(value << 4);
        value = (byte)((value >> 4) | lower);

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, value == 0);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY | (byte)RegisterFlags.FLAG_CARRY, false);
    }


    void extended_execute(ushort opcode)
    {
        mmu._clock.t_instr += extendedInstructionTicks[opcode];
        string text = ("OP Code: 0x" + opcode.ToString("X2"));
        debugLabel.GetComponent<TextMeshProUGUI>().text = text;

        switch (opcode)
        {
            case 0x00: // RLC B
                tempBytes = registers.BC.b;
                rlc(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x01: // RLC C
                tempBytes = registers.BC.c;
                rlc(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x02: // RLC D
                byte rld = registers.DE.d;
                rlc(ref rld);
                registers.DE.d = rld;
                break;
            case 0x03: // RLC E
                byte rlce = registers.DE.e;
                rlc(ref rlce);
                registers.DE.e = rlce;
                break;
            case 0x04: // RLC H
                tempBytes = registers.HL.h;
                rlc(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x05: // RLC L
                tempBytes = registers.HL.l;
                rlc(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x06: // RLC (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    rlc(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    Debug.Log("Inst WRITE");

                    break;
                }
            case 0x07: // RLC A
                tempBytes = registers.AF.a;
                rlc(ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            case 0x08: // RRC B
                tempBytes = registers.BC.b;
                rrc(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x09: // RRC C
                tempBytes = registers.BC.c;
                rrc(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x0A: // RRC D
                byte rrcd = registers.DE.d;
                rrc(ref rrcd);
                registers.DE.d = rrcd;
                break;
            case 0x0B: // RRC E
                byte rrce = registers.DE.e;
                rrc(ref rrce);
                registers.DE.e = rrce;
                break;
            case 0x0C: // RRC H
                tempBytes = registers.HL.h;
                rrc(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x0D: // RRC L
                tempBytes = registers.HL.l;
                rrc(ref tempBytes);
                break;
            case 0x0E: // RRC (HL)
                {
                    byte value = mmu.read_byte(registers.HL.hl);
                    rrc(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x0F: // RRC A
                tempBytes = registers.AF.a;
                rrc(ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            case 0x10: // RL B
                tempBytes = registers.BC.b;
                rl(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x11: // RL C
                tempBytes = registers.BC.c;
                rl(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x12: // RL D
                byte rldd = registers.DE.d;
                rl(ref rldd);
                registers.DE.d = rldd;
                break;
            case 0x13: // RL E
                byte rle = registers.DE.e;
                rl(ref rle);
                registers.DE.e = rle;
                break;
            case 0x14: // RL H
                tempBytes = registers.HL.h;
                rl(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x15: // RL L
                tempBytes = registers.HL.l;
                rl(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x16: // RL (HL)
                {
                    byte value = mmu.read_byte(registers.HL.hl);
                    rl(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x17: // RL A
                tempBytes = registers.AF.a;
                rl(ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0x18: // RR B
                tempBytes = registers.BC.b;
                rr(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x19: // RR C
                tempBytes = registers.BC.c;
                rr(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x1A: // RR D
                byte rrd = registers.DE.d;
                rr(ref rrd);
                registers.DE.d = rrd;
                break;
            case 0x1B: // RR E
                byte rre = registers.DE.e;
                rr(ref rre);
                registers.DE.e = rre;
                break;
            case 0x1C: // RR H
                tempBytes = registers.HL.h;
                rr(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x1D: // RR L
                tempBytes = registers.HL.l;
                rr(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x1E: // RR (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    rr(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x1F: // RR A
                tempBytes = registers.AF.a;
                rr(ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            case 0x20: // SLA B
                tempBytes = registers.BC.b;
                sla(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x21: // SLA C
                tempBytes = registers.BC.c;
                sla(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x22: // SLA D
                byte slad = registers.DE.d;
                sla(ref slad);
                registers.DE.d = slad;
                break;
            case 0x23: // SLA E
                byte slae = registers.DE.e;
                sla(ref slae);
                registers.DE.e = slae;
                break;
            case 0x24: // SLA H
                tempBytes = registers.HL.h;
                sla(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x25: // SLA L
                tempBytes = registers.HL.l;
                sla(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x26: // SLA (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    sla(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x27: // SRA A
                tempBytes = registers.AF.a;
                sla(ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0x28: // SRA B
                tempBytes = registers.BC.b;
                sra(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x29: // SRA C
                tempBytes = registers.BC.c;
                sra(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x2A: // SRA D
                byte srad = registers.DE.d;
                sra(ref srad);
                registers.DE.d = srad;
                break;
            case 0x2B: // SRA E
                byte srae = registers.DE.e;
                sra(ref srae);
                registers.DE.e = srae;
                break;
            case 0x2C: // SRA H
                tempBytes = registers.HL.h;
                sra(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x2D: // SRA L
                tempBytes = registers.HL.l;
                sra(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x2E: // SRA (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    sra(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x2F: // SRA A
                tempBytes = registers.AF.a;
                sra(ref tempBytes);
                registers.AF.a = tempBytes;                
                break;
            case 0x30: // SWAP B
                tempBytes = registers.BC.b;
                swap(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x31: // SWAP C
                tempBytes = registers.BC.c;
                swap(ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x32: // SWAP D
                byte swapd = registers.DE.d;
                swap(ref swapd);
                registers.DE.d = swapd;
                break;
            case 0x33: // SWAP E
                byte swape = registers.DE.e;
                swap(ref swape);
                registers.DE.e = swape;
                break;
            case 0x34: // SWAP H
                tempBytes = registers.HL.h;
                swap(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x35: // SWAP L
                tempBytes = registers.HL.l;
                swap(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x36: // SWAP (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    swap(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x37: // SWAP A
                tempBytes = registers.AF.a;
                swap(ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            case 0x38: // SRL B
                tempBytes = registers.BC.b;
                srl(ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x39: // SRL C
                tempBytes = registers.BC.c;
                srl(ref tempBytes);
                registers.BC.c = tempBytes; 
                break;
            case 0x3A: // SRL D
                byte srld = registers.DE.d;
                srl(ref srld);
                registers.DE.d = srld;
                break;
            case 0x3B: // SRL E
                byte srle = registers.DE.e;
                srl(ref srle);
                registers.DE.e = srle;
                break;
            case 0x3C: // SRL H
                tempBytes = registers.HL.h;
                srl(ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x3D: // SRL L
                tempBytes = registers.HL.l;
                srl(ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x3E: // SRL (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    srl(ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x3F: // SRL A
                tempBytes = registers.AF.a;
                srl(ref tempBytes);
                registers.AF.a = tempBytes ;
                
                break;
            case 0x40: // BIT 0, B
                bit(1 << 0, registers.BC.b);
                break;
            case 0x41: // BIT 0, C
                bit(1 << 0, registers.BC.c);
                break;
            case 0x42: // BIT 0, D
                bit(1 << 0, registers.DE.d);
                break;
            case 0x43: // BIT 0, E
                bit(1 << 0, registers.DE.e);
                break;
            case 0x44: // BIT 0, H
                bit(1 << 0, registers.HL.h);
                break;
            case 0x45: // BIT 0, L
                bit(1 << 0, registers.HL.l);
                break;
            case 0x46: // BIT 0, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 0, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x47: // BIT 0, A
                bit(1 << 0, registers.AF.a);
                
                break;

            case 0x48: // BIT 1, B
                bit(1 << 1, registers.BC.b);
                break;
            case 0x49: // BIT 1, C
                bit(1 << 1, registers.BC.c);
                break;
            case 0x4A: // BIT 1, D
                bit(1 << 1, registers.DE.d);
                break;
            case 0x4B: // BIT 1, E
                bit(1 << 1, registers.DE.e);
                break;
            case 0x4C: // BIT 1, H
                bit(1 << 1, registers.HL.h);
                break;
            case 0x4D: // BIT 1, L
                bit(1 << 1, registers.HL.l);
                break;
            case 0x4E: // BIT 1, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 1, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x4F: // BIT 1, A
                bit(1 << 1, registers.AF.a);
                
                break;
            case 0x50: // BIT 2, B
                bit(1 << 2, registers.BC.b);
                break;
            case 0x51: // BIT 2, C
                bit(1 << 2, registers.BC.c);
                break;
            case 0x52: // BIT 2, D
                bit(1 << 2, registers.DE.d);
                break;
            case 0x53: // BIT 2, E
                bit(1 << 2, registers.DE.e);
                break;
            case 0x54: // BIT 2, H
                bit(1 << 2, registers.HL.h);
                break;
            case 0x55: // BIT 2, L
                bit(1 << 2, registers.HL.l);
                break;
            case 0x56: // BIT 2, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 2, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x57: // BIT 2, A
                bit(1 << 2, registers.AF.a);
                
                break;
            case 0x58: // BIT 3, B
                bit(1 << 3, registers.BC.b);
                break;
            case 0x59: // BIT 3, C
                bit(1 << 3, registers.BC.c);
                break;
            case 0x5A: // BIT 3, D
                bit(1 << 3, registers.DE.d);
                break;
            case 0x5B: // BIT 3, E
                bit(1 << 3, registers.DE.e);
                break;
            case 0x5C: // BIT 3, H
                bit(1 << 3, registers.HL.h);
                break;
            case 0x5D: // BIT 3, L
                bit(1 << 3, registers.HL.l);
                break;
            case 0x5E: // BIT 3, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 3, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x5F: // BIT 3, A
                bit(1 << 3, registers.AF.a);
                
                break;
            case 0x60: // BIT 4, B
                bit(1 << 4, registers.BC.b);
                break;
            case 0x61: // BIT 4, C
                bit(1 << 4, registers.BC.c);
                break;
            case 0x62: // BIT 4, D
                bit(1 << 4, registers.DE.d);
                break;
            case 0x63: // BIT 4, E
                bit(1 << 4, registers.DE.e);
                break;
            case 0x64: // BIT 4, H
                bit(1 << 4, registers.HL.h);
                break;
            case 0x65: // BIT 4, L
                bit(1 << 4, registers.HL.l);
                break;
            case 0x66: // BIT 4, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 4, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x67: // BIT 4, A
                bit(1 << 4, registers.AF.a);
                
                break;
            case 0x68: // BIT 5, B
                bit(1 << 5, registers.BC.b);
                break;
            case 0x69: // BIT 5, C
                bit(1 << 5, registers.BC.c);
                break;
            case 0x6A: // BIT 5, D
                bit(1 << 5, registers.DE.d);
                break;
            case 0x6B: // BIT 5, E
                bit(1 << 5, registers.DE.e);
                break;
            case 0x6C: // BIT 5, H
                bit(1 << 5, registers.HL.h);
                break;
            case 0x6D: // BIT 5, L
                bit(1 << 5, registers.HL.l);
                break;
            case 0x6E: // BIT 5, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 5, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x6F: // BIT 5, A
                bit(1 << 5, registers.AF.a);
                
                break;
            case 0x70: // BIT 6, B
                bit(1 << 6, registers.BC.b);
                break;
            case 0x71: // BIT 6, C
                bit(1 << 6, registers.BC.c);
                break;
            case 0x72: // BIT 6, D
                bit(1 << 6, registers.DE.d);
                break;
            case 0x73: // BIT 6, E
                bit(1 << 6, registers.DE.e);
                break;
            case 0x74: // BIT 6, H
                bit(1 << 6, registers.HL.h);
                break;
            case 0x75: // BIT 6, L
                bit(1 << 6, registers.HL.l);
                break;
            case 0x76: // BIT 6, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 6, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x77: // BIT 6, A
                bit(1 << 6, registers.AF.a);
                
                break;
            case 0x78: // BIT 7, B
                bit(1 << 7, registers.BC.b);
                break;
            case 0x79: // BIT 7, C
                bit(1 << 7, registers.BC.c);
                break;
            case 0x7A: // BIT 7, D
                bit(1 << 7, registers.DE.d);
                break;
            case 0x7B: // BIT 7, E
                bit(1 << 7, registers.DE.e);
                break;
            case 0x7C: // BIT 7, H
                bit(1 << 7, registers.HL.h);
                break;
            case 0x7D: // BIT 7, L
                bit(1 << 7, registers.HL.l);
                break;
            case 0x7E: // BIT 7, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    bit(1 << 7, value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x7F: // BIT 7, A
                bit(1 << 7, registers.AF.a);
                
                break;
            case 0x80: // RES 0, B
                tempBytes = registers.BC.b;
                res(0x1 << 0, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x81: // RES 0, C
                tempBytes = registers.BC.c;
                res(1 << 0, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x82: // RES 0, D
                tempBytes = registers.DE.d;
                res(1 << 0, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0x83: // RES 0, E
                tempBytes = registers.DE.e;
                res(1 << 0, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0x84: // RES 0, H
                tempBytes = registers.HL.h;
                res(1 << 0, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x85: // RES 0, L
                tempBytes = registers.HL.l;
                res(1 << 0, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x86: // RES 0, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 0, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x87: // RES 0, A
                tempBytes = registers.AF.a;
                res(1 << 0, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;

            case 0x88: // RES 1, B
                tempBytes = registers.BC.b;
                res(1 << 1, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x89: // RES 1, C
                tempBytes = registers.BC.c;
                res(1 << 1,ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x8A: // RES 1, D
                tempBytes = registers.DE.d;
                res(1 << 1,ref  tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0x8B: // RES 1, E
                tempBytes = registers.DE.e;
                res(1 << 1, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0x8C: // RES 1, H
                tempBytes = registers.HL.h;
                res(1 << 1, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x8D: // RES 1, L
                tempBytes = registers.HL.l;
                res(1 << 1, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x8E: // RES 1, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 1, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x8F: // RES 1, A
                tempBytes = registers.AF.a;
                res(1 << 1, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0x90: // RES 2, B
                tempBytes = registers.BC.b;
                res(1 << 2, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x91: // RES 2, C
                tempBytes = registers.BC.c;
                res(1 << 2, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x92: // RES 2, D
                tempBytes = registers.DE.d;
                res(1 << 2, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0x93: // RES 2, E
                tempBytes = registers.DE.e;
                res(1 << 2, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0x94: // RES 2, H
                tempBytes = registers.HL.h;
                res(1 << 2, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x95: // RES 2, L
                tempBytes = registers.HL.l;
                res(1 << 2, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x96: // RES 2, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 2, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x97: // RES 2, A
                tempBytes = registers.AF.a;
                res(1 << 2, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0x98: // RES 3, B
                tempBytes = registers.BC.b;
                res(1 << 3, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0x99: // RES 3, C
                tempBytes = registers.BC.c;
                res(1 << 3, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0x9A: // RES 3, D
                tempBytes = registers.DE.d;
                res(1 << 3, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0x9B: // RES 3, E
                tempBytes = registers.DE.e;
                res(1 << 3, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0x9C: // RES 3, H
                tempBytes = registers.HL.h;
                res(1 << 3, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0x9D: // RES 3, L
                tempBytes = registers.HL.l;
                res(1 << 3, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0x9E: // RES 3, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 3, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0x9F: // RES 3, A
                tempBytes = registers.AF.a;
                res(1 << 3, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xA0: // RES 4, B
                tempBytes = registers.BC.b;
                res(1 << 4, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xA1: // RES 4, C
                tempBytes = registers.BC.c;
                res(1 << 4, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xA2: // RES 4, D
                tempBytes = registers.DE.d;
                res(1 << 4, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xA3: // RES 4, E
                tempBytes = registers.DE.e;
                res(1 << 4, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xA4: // RES 4, H
                tempBytes = registers.HL.h;
                res(1 << 4, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xA5: // RES 4, L
                tempBytes = registers.HL.l;
                res(1 << 4, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xA6: // RES 4, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 4, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xa7: // RES 4, A
                tempBytes = registers.AF.a;
                res(1 << 4, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xa8: // RES 5, B
                tempBytes = registers.BC.b;
                res(1 << 5, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xa9: // RES 5, C
                tempBytes = registers.BC.c;
                res(1 << 5, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xAA: // RES 5, D
                tempBytes = registers.DE.d;
                res(1 << 5, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xAB: // RES 5, E
                tempBytes = registers.DE.e;
                res(1 << 5, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xAC: // RES 5, H
                tempBytes = registers.HL.h;
                res(1 << 5, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xAD: // RES 5, L
                tempBytes = registers.HL.l;
                res(1 << 5, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xAE: // RES 5, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 5, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xAF: // RES 5, A
                tempBytes = registers.AF.a;
                res(1 << 5, ref tempBytes);
                registers.AF.a = tempBytes;                
                break;
            case 0xB0: // RES 6, B
                tempBytes = registers.BC.b;
                res(1 << 6, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xB1: // RES 6, C
                tempBytes = registers.BC.c;
                res(1 << 6, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xB2: // RES 6, D
                tempBytes = registers.DE.d;
                res(1 << 6, ref tempBytes);
                registers.DE.d  = tempBytes;
                break;
            case 0xB3: // RES 6, E
                tempBytes = registers.DE.e;
                res(1 << 6, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xB4: // RES 6, H
                tempBytes = registers.HL.h;
                res(1 << 6, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xB5: // RES 6, L
                tempBytes = registers.HL.l;
                res(1 << 6, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xB6: // RES 6, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 6, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xb7: // RES 6, A
                tempBytes = registers.AF.a;
                res(1 << 6, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xb8: // RES 7, B
                tempBytes = registers.BC.b;
                res(1 << 7, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xb9: // RES 7, C
                tempBytes = registers.BC.c;
                res(1 << 7, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xBA: // RES 7, D
                tempBytes = registers.DE.d;
                res(1 << 7, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xBB: // RES 7, E
                tempBytes = registers.DE.e;
                res(1 << 7, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xBC: // RES 7, H
                tempBytes = registers.HL.h;
                res(1 << 7, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xBD: // RES 7, L
                tempBytes = registers.HL.l;
                res(1 << 7, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xBE: // RES 7, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    res(1 << 7, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xBF: // RES 7, A
                tempBytes = registers.AF.a;
                res(1 << 7, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xC0: // SET 0, B
                tempBytes = registers.BC.b;
                set(1 << 0, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xC1: // SET 0, C
                tempBytes = registers.BC.c;
                set(1 << 0, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xC2: // SET 0, D
                tempBytes = registers.DE.d;
                set(1 << 0, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xC3: // SET 0, E
                tempBytes = registers.DE.e;
                set(1 << 0, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xC4: // SET 0, H
                tempBytes = registers.HL.h;
                set(1 << 0, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xC5: // SET 0, L
                tempBytes = registers.HL.l;
                set(1 << 0, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xC6: // SET 0, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 0, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xc7: // SET 0, A
                tempBytes = registers.AF.a;
                set(1 << 0, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xc8: // SET 1, B
                tempBytes = registers.BC.b;
                set(1 << 1, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xc9: // SET 1, C
                tempBytes = registers.BC.c;
                set(1 << 1, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xCA: // SET 1, D
                tempBytes = registers.DE.d;
                set(1 << 1, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xCB: // SET 1, E
                tempBytes = registers.DE.e;
                set(1 << 1, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xCC: // SET 1, H
                tempBytes = registers.HL.h;
                set(1 << 1, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xCD: // SET 1, L
                tempBytes = registers.HL.l;
                set(1 << 1, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xCE: // SET 1, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 1, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xCF: // SET 1, A
                tempBytes = registers.AF.a;
                set(1 << 1, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xD0: // SET 2, B
                tempBytes = registers.BC.b;
                set(1 << 2, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xD1: // SET 2, C
                tempBytes = registers.BC.c;
                set(1 << 2, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xD2: // SET 2, D
                tempBytes = registers.DE.d;
                set(1 << 2,ref  tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xD3: // SET 2, E
                tempBytes = registers.DE.d;
                set(1 << 2, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xD4: // SET 2, H
                tempBytes = registers.HL.h;
                set(1 << 2, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xD5: // SET 2, L
                tempBytes = registers.HL.l;
                set(1 << 2, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xD6: // SET 2, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 2, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xD7: // SET 2, A
                tempBytes = registers.AF.a;
                set(1 << 2, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xD8: // SET 3, B
                tempBytes = registers.BC.b;
                set(1 << 3, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xD9: // SET 3, C
                tempBytes = registers.BC.c;
                set(1 << 3, ref tempBytes);
                registers.BC.c = tempBytes; 
                break;
            case 0xDA: // SET 3, D
                tempBytes = registers.DE.d;
                set(1 << 3, ref tempBytes);
                registers.DE.d = tempBytes; 
                break;
            case 0xDB: // SET 3, E
                tempBytes = registers.DE.e;
                set(1 << 3,ref  tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xDC: // SET 3, H
                tempBytes = registers.HL.h;
                set(1 << 3, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xDD: // SET 3, L
                tempBytes = registers.HL.l;
                set(1 << 3,ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xDE: // SET 3, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 3, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xDF: // SET 3, A
                tempBytes = registers.AF.a;
                set(1 << 3, ref tempBytes);
                registers.AF.a = tempBytes;                
                break;
            case 0xE0: // SET 4, B
                tempBytes = registers.BC.b;
                set(1 << 4, ref  tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xE1: // SET 4, C
                tempBytes = registers.BC.c;
                set(1 << 4, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xE2: // SET 4, D
                tempBytes = registers.DE.d;
                set(1 << 4, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xE3: // SET 4, E
                tempBytes = registers.DE.e;
                set(1 << 4, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xE4: // SET 4, H
                tempBytes = registers.HL.h;
                set(1 << 4, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xE5: // SET 4, L
                tempBytes = registers.HL.l;
                set(1 << 4, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xE6: // SET 4, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 4, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xE7: // SET 4, A
                tempBytes = registers.AF.a;
                set(1 << 4, ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            case 0xE8: // SET 5, B
                tempBytes = registers.BC.b;
                set(1 << 5, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xE9: // SET 5, C
                tempBytes = registers.BC.c;
                set(1 << 5, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xEA: // SET 5, D
                tempBytes = registers.DE.d;
                set(1 << 5,ref  tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xEB: // SET 5, E
                tempBytes = registers.DE.e;
                set(1 << 5, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xEC: // SET 5, H
                tempBytes = registers.HL.h;
                set(1 << 5, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xED: // SET 5, L
                tempBytes = registers.HL.l;
                set(1 << 5, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xEE: // SET 5, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 5, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xEF: // SET 5, A
                tempBytes = registers.AF.a;
                set(1 << 5, ref tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xF0: // SET 6, B
                tempBytes = registers.BC.b;
                set(1 << 6, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xF1: // SET 6, C
                tempBytes = registers.BC.c;
                set(1 << 6, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xF2: // SET 6, D
                tempBytes = registers.DE.d;
                set(1 << 6, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xF3: // SET 6, E
                tempBytes = registers.DE.e;
                set(1 << 6, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xF4: // SET 6, H
                tempBytes = registers.HL.h;
                set(1 << 6,ref  tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xF5: // SET 6, L
                tempBytes = registers.HL.l;
                set(1 << 6, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xF6: // SET 6, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 6, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xF7: // SET 6, A
                tempBytes = registers.AF.a;
                set(1 << 6,ref  tempBytes);
                registers.AF.a = tempBytes;
                
                break;
            case 0xF8: // SET 7, B
                tempBytes = registers.BC.b;
                set(1 << 7, ref tempBytes);
                registers.BC.b = tempBytes;
                break;
            case 0xF9: // SET 7, C
                tempBytes = registers.BC.c;
                set(1 << 7, ref tempBytes);
                registers.BC.c = tempBytes;
                break;
            case 0xFA: // SET 7, D
                tempBytes = registers.DE.d;
                set(1 << 7, ref tempBytes);
                registers.DE.d = tempBytes;
                break;
            case 0xFB: // SET 7, E
                tempBytes = registers.DE.e;
                set(1 << 7, ref tempBytes);
                registers.DE.e = tempBytes;
                break;
            case 0xFC: // SET 7, H
                tempBytes = registers.HL.h;
                set(1 << 7, ref tempBytes);
                registers.HL.h = tempBytes;
                break;
            case 0xFD: // SET 7, L
                tempBytes = registers.HL.l;
                set(1 << 7, ref tempBytes);
                registers.HL.l = tempBytes;
                break;
            case 0xFE: // SET 7, (HL)
                {
                    
                    byte value = mmu.read_byte(registers.HL.hl);
                    set(1 << 7, ref value);
                    mmu.write_byte(registers.HL.hl, value);
                    break;
                }
            case 0xFF: // SET 7, A
                tempBytes = registers.AF.a;
                set(1 << 7, ref tempBytes);
                registers.AF.a = tempBytes;
                break;
            default:
                Debug.Log("Unsupported CB opcode: 0x%02x at 0x%04x\n\n\n" + opcode + " "+ this.registers.pc);
                return;
                
        }
    }

    public void execute(byte opcode)
    {
        mmu._clock.t_instr += instructionTicks[opcode];
          //  string text = ("OP Code: 0x" + opcode.ToString("X2"));
           Debug.Log("OP Code: 0x" + opcode.ToString("X2") + "\n Address: 0x"+ (registers.pc - 1).ToString("X2")) ;
       // debugLabel.GetComponent<TextMeshProUGUI>().text = text;


        switch (opcode)
            {
                case 0x00: // NOP
                    break;
                case 0x01: // LD BC, nn
                    registers.BC.bc = mmu.read_short(registers.pc);
                    registers.pc += 2;
                    break;
                case 0x02: // LD (BC), A
                    
                    
                    mmu.write_byte(registers.BC.bc, registers.AF.a);
                   
                break;
                case 0x03: // INC BC
            

                registers.BC.bc++;
                    break;
                case 0x04: // INC B
                    registers.BC.b += inc(registers.BC.b);
                    break;
                case 0x05: // DEC B
                    dec(registers.BC.b);
                    registers.BC.b--;
                break;
                case 0x06: // LD B, n
                    registers.BC.b = mmu.read_byte(registers.pc++);
                    break;
                case 0x07: // RLCA
                    tempBytes = registers.AF.a;
                    rlc(ref tempBytes);
                    registers.AF.a = tempBytes;
                    
                    registers.set_flags((byte)RegisterFlags.FLAG_ZERO, false);
                    break;
                case 0x08: // LD (nn), SP
                    mmu.write_short(mmu.read_short(registers.pc), registers.sp);
                    registers.pc += 2;
                    break;
                case 0x09: // ADD HL, BC
                    ushort addr = registers.HL.hl;
                    add(ref addr, (byte)registers.BC.bc);
                    registers.HL.hl = addr;
                    break;
                case 0x0A: // LD A, (BC)
                    registers.AF.a = mmu.read_byte(registers.BC.bc);
                    
                    break;
                case 0x0B: // DEC BC
                    registers.BC.bc--;
                    break;
                case 0x0C: // INC C
                registers.BC.c += inc(registers.BC.c);
                    break;
                case 0x0D: // DEC C
                    
                    dec(registers.BC.c);
                    registers.BC.c--;
                    break;
                case 0x0E: // LD C, n
                    registers.BC.c = mmu.read_byte(registers.pc++);
                    break;
                case 0x0F: // RRCA
                    tempBytes = registers.AF.a;
                    rrc(ref tempBytes);
                    registers.AF.a = tempBytes;
                    registers.set_flags((byte)RegisterFlags.FLAG_ZERO, false);
                    break;
                case 0x10: // STOP
                    break;
                case 0x11: // LD DE, nn
                    registers.DE.de = mmu.read_short(registers.pc);
                    registers.pc += 2;
                    break;
                case 0x12: // LD (DE), A
                    mmu.write_byte(registers.DE.de, registers.AF.a);
                    break;
                case 0x13: // INC DE

                registers.DE.de++;
                    break;
                case 0x14: // INC D
                    registers.DE.d += inc(registers.DE.d);
                    break;
                case 0x15: // DEC D
                    dec(registers.DE.d);
                    registers.DE.d--;
                    break;
                case 0x16: // LD D, n
                    Debug.Log("LD D, n");
                    registers.DE.d = mmu.read_byte(registers.pc++);
                    break;
                case 0x17: // RLA
                    tempBytes = registers.AF.a;
                    rl(ref tempBytes);
                    registers.AF.a = tempBytes;
                    registers.set_flags((byte)RegisterFlags.FLAG_ZERO, false);
                    break;
                case 0x18: // JR nn
                    {
                        byte operand = mmu.read_byte(registers.pc++);
                        registers.pc += (ushort)((sbyte)(operand));
                    }
                    break;
                case 0x19: // ADD HL, DE
                    ushort addhlde = registers.HL.hl;
                    add(ref addhlde, (byte)registers.DE.de);
                    registers.HL.hl = addhlde;
                    break;
                case 0x1A: // LD A, (DE)
                    registers.AF.a = mmu.read_byte(registers.DE.de);
                    break;
                case 0x1B: // DEC DE
                    registers.DE.de--;
                    break;
                case 0x1C: // INC E
                    
                    registers.DE.e += inc(registers.DE.e);
                    
                    break;
                case 0x1D: // DEC E
                    dec(registers.DE.e);
                    registers.DE.e--;
                    break;
                case 0x1E: // LD E, n
                    registers.DE.e = mmu.read_byte(registers.pc++);
                    break;
                case 0x1F: // RRA
                    tempBytes = registers.AF.a;
                    rr(ref tempBytes);
                    registers.AF.a = tempBytes;
                    registers.set_flags((byte)RegisterFlags.FLAG_ZERO, false);
                    break;
                case 0x20: // JR NZ, *
                    bool flagset = registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO);
                    jump_add(!flagset);
                    break;
                case 0x21: // LD HL, nn
                
                registers.HL.hl = mmu.read_short(registers.pc);
                    registers.pc += 2;
                    break;
                case 0x22: // LD (HLI), A | LD (HL+), A | LDI (HL), A
                    
                    mmu.write_byte(registers.HL.hl++, registers.AF.a);
                    break;
                case 0x23: // INC HL
                           //registers.HL.inc();
                    

                    registers.HL.hl++;
                    break;
                case 0x24: // INC H

                    registers.HL.h += inc(registers.HL.h);
                    break;
                case 0x25: // DEC H
                    dec(registers.HL.h);
                    registers.HL.h--;
                    break;
                case 0x26: // LD H, n
                    registers.HL.h = mmu.read_byte(registers.pc++);
                    break;
                case 0x27: // DAA
                    {
                        byte value = registers.AF.a;

                        if (registers.is_flag_set((byte)RegisterFlags.FLAG_SUBTRACT))
                        {
                            if (registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY))
                            {
                                value -= 0x60;
                            }

                            if (registers.is_flag_set((byte)RegisterFlags.FLAG_HALF_CARRY))
                            {
                                value -= 0x6;
                            }
                        }
                        else
                        {
                            if (registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY) || value > 0x99)
                            {
                                value += 0x60;
                                registers.set_flags((byte)RegisterFlags.FLAG_CARRY, true);
                            }

                            if (registers.is_flag_set((byte)RegisterFlags.FLAG_HALF_CARRY) || (value & 0xF) > 0x9)
                            {
                                value += 0x6;
                            }

                        }
                        registers.AF.a = value;

                        registers.set_flags((byte)RegisterFlags.FLAG_ZERO, registers.AF.a == 0);
                        registers.set_flags((byte)RegisterFlags.FLAG_HALF_CARRY, false);
                        break;
                    }
                case 0x28: // JR Z, *
                    jump_add(registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0x29: // ADD HL, HL
                    ushort addhlhl = registers.HL.hl;
                    add(ref addhlhl, (byte)registers.HL.hl);
                    registers.HL.hl = addhlhl;
                    break;
                case 0x2A: // LD A, (HL+)
                
                registers.AF.a = mmu.read_byte(registers.HL.hl++);
                    break;
                case 0x2B: // DEC HL
                
                registers.HL.hl--;
                    break;
                case 0x2C: // INC L
                    registers.HL.l += inc(registers.HL.l);
                    break;
                case 0x2D: // DEC L
                    dec(registers.HL.l);
                    registers.HL.l--;
                    break;
                case 0x2E: // LD L, n
                    registers.HL.l = mmu.read_byte(registers.pc++);
                    break;
                case 0x2F: // CPL
                    registers.AF.a = (byte)~registers.AF.a;
                    registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, true);
                    break;
                case 0x30: // JR NC, *
                    jump_add(!registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0x31: // LD SP, nn
                    Debug.Log("LD SP, nn");
                    registers.sp = mmu.read_short(registers.pc);
                    registers.pc += 2;
                    break;
                case 0x32: // LD (HLD), A | LD (HL-), A | LDD (HL), A
                
                    mmu.write_byte(registers.HL.hl--, registers.AF.a);
                    break;
                case 0x33: // INC SP
                    registers.sp++;
                    break;
                case 0x35: // DEC (HL)
                    {
                        byte tmp_val; 
                        tmp_val = mmu.read_byte(registers.HL.hl);
                        dec(tmp_val);
                        mmu.write_byte(registers.HL.hl, tmp_val--);
                        break;
                    }
                case 0x34: // INC (HL)
                    {
                        
                        byte tmp_val = 0;
                        tmp_val = mmu.read_byte(registers.HL.hl);
                        tmp_val += inc(tmp_val);
                        mmu.write_byte(registers.HL.hl, (byte)tmp_val);
                        break;
                    }
                case 0x36: // LD (HL), n
                
                mmu.write_byte(registers.HL.hl, mmu.read_byte(registers.pc++));
                    break;
                case 0x37: // SCF
                    registers.set_flags((byte)RegisterFlags.FLAG_CARRY, true);
                    registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
                    break;
                case 0x38: // JR C, *
                    jump_add(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0x39: // ADD HL, SP
                    ushort addhlsp = registers.HL.hl;
                    add(ref addhlsp, (byte)registers.sp);
                    registers.HL.hl = addhlsp;
                    break;
                case 0x3A: // LD A, (HL-)
                
                registers.AF.a = mmu.read_byte(registers.HL.hl--);
                    break;
                case 0x3B: // DEC SP
                    registers.sp--;
                    break;
                case 0x3C: // INC A
                    registers.AF.a += inc(registers.AF.a);
                    break;
                case 0x3D: // DEC A
                    dec(registers.AF.a);
                    registers.AF.a--;
                    break;
                case 0x3E: // LD A, n
                    registers.AF.a = mmu.read_byte(registers.pc++);
                    break;
                case 0x3F: // CCF
                    registers.set_flags((byte)RegisterFlags.FLAG_CARRY, !registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));

                    registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT | (byte)RegisterFlags.FLAG_HALF_CARRY, false);
                    break;
                case 0x40: // LD B, B
                    break;
                case 0x41: // LD B, C
                    registers.BC.b = registers.BC.c;
                    break;
                case 0x42: // LD B, D
                    registers.BC.b = registers.DE.d;
                    break;
                case 0x43: // LD B, E
                    registers.BC.b = registers.DE.e;
                    break;
                case 0x44: // LD B, H
                    registers.BC.b = registers.HL.h;
                    break;
                case 0x45: // LD B, L
                    registers.BC.b = registers.HL.l;
                    break;
                case 0x46: // LD B, (HL)
                
                registers.BC.b = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x47: // LD B, A
                    registers.BC.b = registers.AF.a;
                    break;
                case 0x48: // LD C, B
                    registers.BC.c = registers.BC.b;
                    break;
                case 0x49: // LD C, C
                    break;
                case 0x4A: // LD C, D
                    registers.BC.c = registers.DE.d;
                    break;
                case 0x4B: // LD C, E
                    registers.BC.c = registers.DE.e;
                    break;
                case 0x4C: // LD C, H
                    registers.BC.c = registers.HL.h;
                    break;
                case 0x4D: // LD C, L
                    registers.BC.c = registers.HL.l;
                    break;
                case 0x4E: // LD C, (HL)
                
                registers.BC.c = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x4F: // LD C, A
                    registers.BC.c = registers.AF.a;
                    break;
                case 0x50: // LD D, B
                    registers.DE.d = registers.BC.b;
                    break;
                case 0x51: // LD D, C
                    registers.DE.d = registers.BC.c;
                    break;
                case 0x52: // LD D, D
                    break;
                case 0x53: // LD D, E
                    registers.DE.d = registers.DE.e;
                    break;
                case 0x54: // LD D, H
                    registers.DE.d = registers.HL.h;
                    break;
                case 0x55: // LD D, L
                    registers.DE.d = registers.HL.l;
                    break;
                case 0x56: // LD D, (HL)
                
                registers.DE.d = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x57: // LD D, A
                    registers.DE.d = registers.AF.a;
                    break;
                case 0x58: // LD E, B
                    registers.DE.e = registers.BC.b;
                    break;
                case 0x59: // LD E, C
                    registers.DE.e = registers.BC.c;
                    break;
                case 0x5A: // LD E, D
                    registers.DE.e = registers.DE.d;
                    break;
                case 0x5B: // LD E, E
                    break;
                case 0x5C: // LD E, H
                    registers.DE.e = registers.HL.h;
                    break;
                case 0x5D: // LD E, L
                    registers.DE.e = registers.HL.l;
                    break;
                case 0x5E: // LD E, (HL)
                
                registers.DE.e = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x5F: // LD E, A
                    registers.DE.e = registers.AF.a;
                    break;
                case 0x60: // LD H, B
                    registers.HL.h = registers.BC.b;
                    break;
                case 0x61: // LD H, C
                    registers.HL.h = registers.BC.c;
                    break;
                case 0x62: // LD H, D
                    registers.HL.h = registers.DE.d;
                    break;
                case 0x63: // LD H, E
                    registers.HL.h = registers.DE.e;
                    break;
                case 0x64: // LD H, H
                    break;
                case 0x65: // LD H, L
                    registers.HL.h = registers.HL.l;
                    break;
                case 0x66: // LD H, (HL)
                
                registers.HL.h = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x67: // LD H, A
                    registers.HL.h = registers.AF.a;
                    break;
                case 0x68: // LD L, B
                    registers.HL.l = registers.BC.b;
                    break;
                case 0x69: // LD L, C
                    registers.HL.l = registers.BC.c;
                    break;
                case 0x6A: // LD L, D
                    registers.HL.l = registers.DE.d;
                    break;
                case 0x6B: // LD L, E
                    registers.HL.l = registers.DE.e;
                    break;
                case 0x6C: // LD L, H
                    registers.HL.l = registers.HL.h;
                    break;
                case 0x6D: // LD L, L
                    break;
                case 0x6E: // LD L, (HL)
                
                registers.HL.l = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x6F: // LD L, A
                    registers.HL.l = registers.AF.a;
                    break;
                case 0x70: // LD (HL), B
                
                mmu.write_byte(registers.HL.hl, registers.BC.b);
                    break;
                case 0x71: // LD (HL), C
                
                mmu.write_byte(registers.HL.hl, registers.BC.c);
                    break;
                case 0x72: // LD (HL), D
                
                mmu.write_byte(registers.HL.hl, registers.DE.d);
                    break;
                case 0x73: // LD (HL), E
                
                mmu.write_byte(registers.HL.hl, registers.DE.e);
                    break;
                case 0x74: // LD (HL), H
                
                mmu.write_byte(registers.HL.hl, registers.HL.h);
                    break;
                case 0x75: // LD (HL), L
                
                mmu.write_byte(registers.HL.hl, registers.HL.l);
                    break;
                case 0x76: // HALT
                    if (!interrupts.is_master_enabled() && ((mmu.read_byte(0xFF0F) & mmu.read_byte(0xFFFF) & 0x1F)) != 0 ? true: false)
                    {
                        mmu.trigger_halt_bug = true;
                        mmu.is_halted = false;
                    }
                    else
                        mmu.is_halted = true;
                    break;
                case 0x77: // LD (HL), A
                
                mmu.write_byte(registers.HL.hl, registers.AF.a);
                    break;
                case 0x78: // LD A, B
                    registers.AF.a = registers.BC.b;
                    break;
                case 0x79: // LD A, C
                    registers.AF.a = registers.BC.c;
                    break;
                case 0x7A: // LD A, D
                    registers.AF.a = registers.DE.d;
                    break;
                case 0x7B: // LD A, E
                    registers.AF.a = registers.DE.e;
                    break;
                case 0x7C: // LD A, H
                    registers.AF.a = registers.HL.h;
                    break;
                case 0x7D: // LD A, L
                    registers.AF.a = registers.HL.l;
                    break;
                case 0x7E: // LD A, (HL)
                
                    registers.AF.a = mmu.read_byte(registers.HL.hl);
                    break;
                case 0x7F: // LD A, A
                    break;
                case 0x80: // ADD A, B
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.BC.b);
                    registers.AF.a = tempBytes;
                    break;
                case 0x81: // ADD A, C
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.BC.c);
                    registers.AF.a = tempBytes;
                    break;
                case 0x82: // ADD A, D
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.DE.d);
                    registers.AF.a = tempBytes;
                    break;
                case 0x83: // ADD A, E
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.DE.e);
                    registers.AF.a = tempBytes;
                    break;
                case 0x84: // ADD A, H
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.HL.h);
                    registers.AF.a = tempBytes;
                    break;
                case 0x85: // ADD A, L
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.HL.l);
                    registers.AF.a = tempBytes;
                    break;
                case 0x86: // ADD A, (HL)
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, mmu.read_byte(registers.HL.hl));
                    registers.AF.a = tempBytes;
                    break;
                case 0x87: // ADD A, A
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, registers.AF.a);
                    registers.AF.a = tempBytes;
                    break;
                case 0x88: // ADC A, B
                    adc(registers.BC.b);
                    break;
                case 0x89: // ADC A, C
                    adc(registers.BC.c);
                    break;
                case 0x8A: // ADC A, D
                    adc(registers.DE.d);
                    break;
                case 0x8B: // ADC A, E
                    adc(registers.DE.e);
                    break;
                case 0x8C: // ADC A, H
                    adc(registers.HL.h);
                    break;
                case 0x8D: // ADC A, L
                    adc(registers.HL.l);
                    break;
                case 0x8E: // ADC A, (HL)
                
                adc(mmu.read_byte(registers.HL.hl));
                    break;
                case 0x8F: // ADC A, A
                    adc(registers.AF.a);
                    break;
                case 0x90: // SUB B
                    sub(registers.BC.b);
                    break;
                case 0x91: // SUB C
                    sub(registers.BC.c);
                    break;
                case 0x92: // SUB D
                    sub(registers.DE.d);
                    break;
                case 0x93: // SUB E
                    sub(registers.DE.e);
                    break;
                case 0x94: // SUB H
                    sub(registers.HL.h);
                    break;
                case 0x95: // SUB L
                    sub(registers.HL.l);
                    break;
                case 0x96: // SUB (HL)
                
                sub(mmu.read_byte(registers.HL.hl));
                    break;
                case 0x97: // SUB A
                    sub(registers.AF.a);
                    break;
                case 0x98: // SBC A, B
                    sbc(registers.BC.b);
                    break;
                case 0x99: // SBC A, C
                    sbc(registers.BC.c);
                    break;
                case 0x9A: // SBC A, D
                    sbc(registers.DE.d);
                    break;
                case 0x9B: // SBC A, E
                    sbc(registers.DE.e);
                    break;
                case 0x9C: // SBC A, H
                    sbc(registers.HL.h);
                    break;
                case 0x9D: // SBC A, L
                    sbc(registers.HL.l);
                    break;
                case 0x9E: // SBC A, (HL)
                
                sbc(mmu.read_byte(registers.HL.hl));
                    break;
                case 0x9F: // SBC A, A
                    sbc(registers.AF.a);
                    break;
                case 0xA0: // AND B
                    and_(registers.BC.b);
                    break;
                case 0xA1: // AND C
                    and_(registers.BC.c);
                    break;
                case 0xA2: // AND D
                    and_(registers.DE.d);
                    break;
                case 0xA3: // AND E
                    and_(registers.DE.e);
                    break;
                case 0xA4: // AND H
                    and_(registers.HL.h);
                    break;
                case 0xA5: // AND l
                    and_(registers.HL.l);
                    break;
                case 0xA6: // AND (HL)
                
                and_(mmu.read_byte(registers.HL.hl));
                    break;
                case 0xA7: // AND A
                    and_(registers.AF.a);
                    break;
                case 0xA8: // XOR B
                    xor_(registers.BC.b);
                    break;
                case 0xA9: // XOR C
                    xor_(registers.BC.c);
                    break;
                case 0xAA: // XOR D
                    xor_(registers.DE.d);
                    break;
                case 0xAB: // XOR E
                    xor_(registers.DE.e);
                    break;
                case 0xAC: // XOR H
                    xor_(registers.HL.h);
                    break;
                case 0xAD: // XOR L
                    xor_(registers.HL.l);
                    break;
                case 0xAE: // XOR (HL)
                
                xor_(mmu.read_byte(registers.HL.hl));
                    break;
                case 0xAF: // XOR A
                    xor_(registers.AF.a);
                    break;
                case 0xB0: // OR B
                    or_(registers.BC.b);
                    break;
                case 0xB1: // OR C
                    or_(registers.BC.c);
                    break;
                case 0xB2: // OR D
                    or_(registers.DE.d);
                    break;
                case 0xB3: // OR E
                    or_(registers.DE.e);
                    break;
                case 0xB4: // OR H
                    or_(registers.HL.h);
                    break;
                case 0xB5: // OR L
                    or_(registers.HL.l);
                    break;
                case 0xB6: // OR (HL)
                
                or_(mmu.read_byte(registers.HL.hl));
                    break;
                case 0xB7: // OR A
                    or_(registers.AF.a);
                    break;
                case 0xB8: // CP B
                    cp(registers.BC.b);
                    break;
                case 0xB9: // CP C
                    cp(registers.BC.c);
                    break;
                case 0xBA: // CP D
                    cp(registers.DE.d);
                    break;
                case 0xBB: // CP E
                    cp(registers.DE.e);
                    break;
                case 0xBC: // CP H
                    cp(registers.HL.h);
                    break;
                case 0xBD: // CP L
                    cp(registers.HL.l);
                    break;
                case 0xBE: // CP (HL)
                    
                    cp(mmu.read_byte(registers.HL.hl));
                    break;
                case 0xBF: // CP A
                    cp(registers.AF.a);
                    break;
                case 0xC0: // RET NZ
                    ret(!registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0xC1: // POP BC
                    registers.BC.bc = mmu.read_short_stack(ref registers.sp);
                    break;
                case 0xC2: // JP NZ, nn
                    jump(!registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0xC3: // JP nn
                    registers.pc = mmu.read_short(registers.pc);
                    break;
                case 0xC4: // CALL NZ, nn
                    call(!registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0xC5: // PUSH BC
                    mmu.write_short_stack(ref registers.sp, registers.BC.bc);
                    break;
                case 0xC6: // ADD A, n 
                    tempBytes = registers.AF.a;
                    add(ref tempBytes, mmu.read_byte(registers.pc++));
                    registers.AF.a = tempBytes;
                    break;
                case 0xC7: // RST $00
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0000;
                    break;
                case 0xC8: // RET Z
                    ret(registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0xC9: // RET
                    registers.pc = mmu.read_short_stack(ref registers.sp);
                break;
                case 0xCA: // JP Z, nn
                    jump(registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;
                case 0xCB:
                    extended_execute(mmu.read_byte(registers.pc++));
                    // registers.pc++;
                    break;
                case 0xCC: // CALL Z, nn
                    call(registers.is_flag_set((byte)RegisterFlags.FLAG_ZERO));
                    break;

                case 0xCD: // CALL nn
                    {
                        ushort operand = mmu.read_short(registers.pc);
                        registers.pc += 2;
                        mmu.write_short_stack(ref registers.sp, registers.pc);
                        registers.pc = operand;
                    }
                    break;
                case 0xCE: // ADC A, n
                    adc(mmu.read_byte(registers.pc++));
                    break;
                case 0xCF: // RST $08
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0008;
                    break;
                case 0xD0: // RET NC
                    ret(!registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xD1: // POP DE
                    registers.DE.de = mmu.read_short_stack(ref registers.sp);
                    break;
                case 0xD2: // JP NC, nn
                    jump(!registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xD4: // CALL NC, nn
                    call(!registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xD5: // PUSH DE
                

                mmu.write_short_stack(ref registers.sp, registers.DE.de);
                    break;
                case 0xD6: // SUB n
                    sub(mmu.read_byte(registers.pc++));
                    break;
                case 0xD7: // RST $10
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0010;
                    break;
                case 0xD8: // RET C
                    ret(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xD9: // RETI
                    interrupts.set_master_flag(true);
                    registers.pc = mmu.read_short_stack(ref registers.sp);
                    break;
                case 0xDA: // JP C, nn
                    jump(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xDC: // CALL C, nn
                    call(registers.is_flag_set((byte)RegisterFlags.FLAG_CARRY));
                    break;
                case 0xDE: // SUB n
                    sbc(mmu.read_byte(registers.pc++));
                    break;
                case 0xDF: // RST $18
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0018;
                    break;
                case 0xE0: // LD ($FF00+n), A
                    mmu.write_byte((ushort)(0xff00 + mmu.read_byte(registers.pc++)), registers.AF.a);
                    break;
                case 0xE1: // POP HL
                    
                    registers.HL.hl = mmu.read_short_stack(ref registers.sp);
                    break;
                case 0xE2: // LD ($FF00+C), A
                    mmu.write_byte((ushort)(0xff00 + registers.BC.c), registers.AF.a);
                    break;
                case 0xE5: // PUSH HL
                
                mmu.write_short_stack(ref registers.sp, registers.HL.hl);
                    break;
                case 0xE6: // AND n
                    and_(mmu.read_byte(registers.pc++));
                    break;
                case 0xE7: // RST $20
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0020;
                    break;
                case 0xE8: // ADD SP, n
                    add(ref registers.sp, mmu.read_byte(registers.pc++));
                    break;
                case 0xE9: // JP HL
                
                    registers.pc = registers.HL.hl;
                    

                    break;
                case 0xEA: // LD (nn), A
                    mmu.write_byte(mmu.read_short(registers.pc), registers.AF.a);
                    registers.pc += 2;
                    break;
                case 0xEE: // XOR n
                    xor_(mmu.read_byte((ushort)(registers.pc + 1)));
                    break;
                case 0xEF: // RST $28
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0028;
                    break;
                case 0xF0: // LD A, ($FF00+n)
                    byte highByte = mmu.read_byte(registers.pc++);
                    ushort result = (ushort)(0xFF00 + highByte);
                    byte dataFound = mmu.read_byte(result);
                    registers.AF.a = dataFound;
                    break;
                case 0xF1: // POP AF
                    registers.AF.af = mmu.read_short_stack(ref registers.sp);
                    registers.AF.f &= 0xf0; // Reset the 4 unused bits
                    break;
                case 0xF2: // LD A, (C)
                    registers.AF.a = mmu.read_byte((ushort)(0xff00 + registers.BC.c));
                    break;
                case 0xF3: // DI
                Debug.Log("DI");
                    interrupts.set_master_flag(false);
                    break;
                case 0xF5: // PUSH AF
                    
                    mmu.write_short_stack(ref registers.sp, registers.AF.af);
                    break;
                case 0xF6: // OR n
                    or_(mmu.read_byte(registers.pc++));
                    break;
                case 0xF7: // RST $30
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0030;
                    break;
                case 0xF8: // LDHL SP, n
                    ldhl((sbyte)mmu.read_byte(registers.pc++));
                    break;
                case 0xF9: // LD SP, HL
                
                    registers.sp = registers.HL.hl;
                    break;
                case 0xFA: // LD A, (nn)
                    registers.AF.a = mmu.read_byte(mmu.read_short(registers.pc));
                    registers.pc += 2;
                    break;
                case 0xFB: // NI
                    interrupts.set_master_flag(true);
                    break;
                case 0xFE: // CP n
                    cp_n(mmu.read_byte(registers.pc++));
                    break;
                case 0xFF: // RST $38
                    mmu.write_short_stack(ref registers.sp, registers.pc++);
                    registers.pc = 0x0038;
                    break;
                default:
                    Debug.Log("FF40: "  +(mmu.read_byte(0xFF40)) + " FF41: "  +(mmu.read_byte(0xFF41)) + " FF42: "  +(mmu.read_byte(0xFF42)) + " FF44: "  +(mmu.read_byte(0xFF44)));
                    registers.print_flags();
                    registers.print_registers();
                    Debug.Log("Unsupported opcode: 0x%02x at 0x%04x\n" + opcode + " " + this.registers.pc);
                    Debug.Log("DIV: %d\n" + mmu._timer.div);
                    Debug.Log("Cycles: %d \n\n\n" + mmu._clock.t);
                    break;
            }
        }
    }


