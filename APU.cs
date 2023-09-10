using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class APU 
{
    public struct  NRx1
    {

        public int length_load;
        public int duty;
        public int value;
        public NRx1(int pop = 1)
        {
            length_load = 6;
            duty = 2;
            value = 0;
        }
    }
    
    public class Frequency
    {
        public struct NRx3
        {
            public int frequency_lsb;// : 8;
            public int value;

            NRx3(int i = 1)
            {
                frequency_lsb = 8;
                value = 0;
            }

        }

        public NRx3 nrx3;
        public struct NRx4
        {
            public int frequency_msb;// : 3;
            public int empty;//: 3;
            public bool length_enable;// : 1;
            public int trigger;// : 1;
            public int value;
            public NRx4(int i = 0)
            {
                frequency_msb = 3;
                empty = 3;
                length_enable = true;
                trigger = 1;
                value = 0;
            }
        }
        public NRx4 nrx4;
        public int duty = 0;
        public int freq_timer = 0;
    }

    public class FrameSequencer
    {
        public int frame_timer;
        public int frame_sequence;

        public int length_timer = 0;

        public bool trigger_length = false;
        public bool trigger_envelope = false;
        public bool trigger_sweep = false;
    };

    public class Envelope
    {
        public class NRx2
        {

            public int period = 3;
            public int direction = 1;
            public int init_vol = 4;

            public int value;
        }
        public NRx2 nrx2;
        public int volume = 0;
        public int timer = 0;
        public bool enabled = false;
    }

    public class Wave
    {
        public class NR30
        {
            public int unused = 7;
            public int dac_power = 1;
            public int value;
        }
       public NR30 nr30;
        public class NR31
        {

            public int length_load = 8;
            public int value;
        } 
        public NR31 nr31;
        public class NR32
        {

            public int empty = 5;
            public int volume = 2;
            public int empty2 = 1;
            public int value;
        } 
        public NR32 nr32;
        public FrameSequencer sequencer;
        public Frequency frequency;
        public int sample = 0;
        public bool enabled;
    }

    public class Square
    {
        public FrameSequencer sequencer;
        public Envelope envelope;
        public Frequency frequency;

        public NRx1 nrx1;
        public bool enabled;
    };

    public class Noise
    {
        public class NR43
        {
            public int divisor = 3;
            public int width_mode = 1;
            public int clock_shift = 4;
            public int value;
        }
        public NR43 nr43;
        public FrameSequencer sequencer;
        public Envelope envelope;
        public Frequency frequency;
        public NRx1 nrx1;
        public int lfsr;
        public bool enabled;
    }

    public Status._Status status;
    public MMU mmu;

    public int[][] duties = new int[4][]
    {
        new int[] { 0, 0, 0, 0, 0, 0, 0, 1 },
        new int[] { 1, 0, 0, 0, 0, 0, 0, 1 },
        new int[] { 1, 0, 0, 0, 0, 1, 1, 1 },
        new int[] { 0, 1, 1, 1, 1, 1, 1, 0 }
    };
    public int[] divisor = { 8, 16, 32, 48, 64, 80, 96, 112 };

    public int audio_freq = 44100;
    public int gameboy_ticks = 4 * 1024 * 1024; // 4194304
    public int sample_rate;

    // Channels
    public Square ch1;
    public Square ch2;
    public Wave wave;
    public Noise noise;
    public Noise ch4;

    public APU(Status._Status status, MMU mmu)
    {
        
        this.sample_rate = gameboy_ticks / audio_freq;
        this.status = status;
        this.mmu = mmu;
        init_audio();
    }

    public void init_audio()
    {
        AudioSource audioSource = new GameObject().AddComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("Audio", audio_freq, 2, audio_freq, false);
        audioSource.Play();

        // Set audio format and channels
        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.sampleRate = audio_freq;
        audioConfig.speakerMode = AudioSpeakerMode.Stereo;
        audioConfig.dspBufferSize = audio_freq / 60;
        AudioSettings.Reset(audioConfig);

        // Attach a callback method to the Unity audio system
        
    }
    void frame_sequencer_action(FrameSequencer sequencer)
    {
        sequencer.frame_timer++;
        if (sequencer.frame_timer == 8192)
        {
            sequencer.frame_timer = 0;
            sequencer.frame_sequence++;
            sequencer.frame_sequence %= 8;

            sequencer.trigger_length = sequencer.frame_sequence % 2 == 0 ? true : false;
            sequencer.trigger_envelope = sequencer.frame_sequence == 7;
            sequencer.trigger_sweep = sequencer.frame_sequence == 2 || sequencer.frame_sequence == 6;
        }
        else
        {
            sequencer.trigger_length = false;
            sequencer.trigger_envelope = false;
            sequencer.trigger_sweep = false;
        }
    }

    bool timer_action(Frequency frequency)
    {
        if (frequency.freq_timer <= 0)
        {
            int wavelen =
                ((frequency.nrx4.frequency_msb << 8) | frequency.nrx3.frequency_lsb);
            frequency.freq_timer = 4 * (2048 - wavelen);
            return true;
        }
        else
            frequency.freq_timer--;
        return false;
    }

    void duty_action(Frequency frequency)
    {
        frequency.duty++;
        frequency.duty %= 8;
    }

    bool length_timer_action(Frequency frequency, FrameSequencer sequencer)
    {
        if (sequencer.trigger_length && frequency.nrx4.length_enable &&
            sequencer.length_timer > 0)
        {
            sequencer.length_timer--;
            if (sequencer.length_timer <= 0)
                return false;
        }
        return true;
    }

    void envelope_action(Envelope envelope, FrameSequencer sequencer)
    {
        if (sequencer.trigger_envelope && envelope.enabled &&
            envelope.nrx2.period != 0)
        {
            envelope.timer--;
            if (envelope.timer <= 0)
            {
                envelope.timer = envelope.nrx2.period;
                int direction = envelope.nrx2.direction != 0 ? 1 : -1;
                int new_volume = envelope.volume + direction;
                if (new_volume >= 0 && new_volume <= 0x0F)
                    envelope.volume = new_volume;
                else
                    envelope.enabled = false;
            }
        }
    }

    int get_ch1_sample()
    {
        // Load all options from memory
        // mmu.read_byte(0xFF15)// NR20
        ch1.nrx1.value = mmu.read_byte(0xFF11);           // NR11
        ch1.envelope.nrx2.value = mmu.read_byte(0xFF12);  // NR12
        ch1.frequency.nrx3.value = mmu.read_byte(0xFF13); // NR13
        ch1.frequency.nrx4.value = mmu.read_byte(0xFF14); // NR14

        if (ch1.frequency.nrx4.trigger != 0)
            reset_ch1();

        bool triggered_timer = timer_action(ch1.frequency);
        if (triggered_timer)
            duty_action(ch1.frequency);
        frame_sequencer_action(ch1.sequencer);

        ch1.enabled &= length_timer_action(ch1.frequency, ch1.sequencer);

        if (ch1.sequencer.trigger_envelope)
            envelope_action(ch1.envelope, ch1.sequencer);

        int sample = duties[ch1.nrx1.duty][ch1.frequency.duty];
        return sample * ch1.envelope.volume * (ch1.enabled ? 1: 0);
    }

    int get_ch2_sample()
    {
        // Load all options from memory
        // mmu.read_byte(0xFF15)// NR20
        ch2.nrx1.value = mmu.read_byte(0xFF16);           // NR21
        ch2.envelope.nrx2.value = mmu.read_byte(0xFF17);  // NR22
        ch2.frequency.nrx3.value = mmu.read_byte(0xFF18); // NR23
        ch2.frequency.nrx4.value = mmu.read_byte(0xFF19); // NR24

        if (ch2.frequency.nrx4.trigger != 0)
            reset_ch2();

        // Execute all the channel actions
        bool triggered_timer = timer_action(ch2.frequency);
        if (triggered_timer)
            duty_action(ch2.frequency);

        frame_sequencer_action(ch2.sequencer);

        ch2.enabled &= length_timer_action(ch2.frequency, ch2.sequencer);

        if (ch2.sequencer.trigger_envelope)
            envelope_action(ch2.envelope, ch2.sequencer);

        // Create sample
        int sample = duties[ch2.nrx1.duty][ch2.frequency.duty];
        return sample * ch2.envelope.volume * (ch2.enabled ? 1 : 0);
    }

    int get_ch3_sample()
    {
        // Load all options from memory
        wave.nr30.value = mmu.read_byte(0xFF1A);           // NR30
        wave.nr31.value = mmu.read_byte(0xFF1B);           // NR32
        wave.nr32.value = mmu.read_byte(0xFF1C);           // NR32
        wave.frequency.nrx3.value = mmu.read_byte(0xFF1D); // NR33
        wave.frequency.nrx4.value = mmu.read_byte(0xFF1E); // NR34

        // Reset channel state
        if (wave.frequency.nrx4.trigger != 0)
            reset_ch3();

        // Execute all the channel actions
        frame_sequencer_action(wave.sequencer);

        // Timer action
        bool triggered_timer = timer_action(wave.frequency);

        // Wave action
        if (triggered_timer)
            wave.sample = (wave.sample + 1) % 32;

        int sample = mmu.read_byte((ushort)(0xff30 + (wave.sample / 2)));
        if (wave.sample % 2 != 0)
            sample = sample & 0xf;
        else
            sample = sample >> 4;

        // Length Counter action
        wave.enabled &= length_timer_action(wave.frequency, wave.sequencer);

        // Volume action
        int shift_vol = wave.nr32.volume != 0 ? wave.nr32.volume - 1 : 4;
        sample >>= shift_vol;

        return sample * (wave.enabled ? 1 : 0) * wave.nr30.dac_power;
    }

    int get_ch4_sample()
    {
        // Load all options from memory
        noise.nrx1.value = mmu.read_byte(0xFF20);           // NR41
        noise.envelope.nrx2.value = mmu.read_byte(0xFF21);  // NR42
        noise.nr43.value = mmu.read_byte(0xFF22);           // NR43
        noise.frequency.nrx4.value = mmu.read_byte(0xFF23); // NR44

        if (noise.frequency.nrx4.trigger != 0)
            reset_ch4();

        // Execute all the channel actions
        frame_sequencer_action(noise.sequencer);

        // Timer and LFSR action
        noise.frequency.freq_timer--;
        if (noise.frequency.freq_timer <= 0)
        {
            noise.frequency.freq_timer = divisor[noise.nr43.divisor]
                                         << noise.nr43.clock_shift;
            int xor_res = (noise.lfsr & 0x1) ^ ((noise.lfsr & 0x2) >> 1);
            noise.lfsr = (noise.lfsr >> 1) | (xor_res << 14);

            if (noise.nr43.width_mode != 0)
            {
                noise.lfsr &= ~(1 << 6);
                noise.lfsr |= (xor_res << 6);
            }
        }

        // Length Counter action
        noise.enabled &= length_timer_action(noise.frequency, noise.sequencer);

        // Envelope action
        envelope_action(noise.envelope, noise.sequencer);

        int sample = (~noise.lfsr & 0x1) * noise.envelope.volume;
        return sample * (noise.enabled ? 1: 0);
    }

    void reset_ch1()
    {
        ch1.frequency.nrx4.trigger = 0;
        mmu.write_byte(0xFF14, (byte)ch1.frequency.nrx4.value);

        ch1.envelope.volume = ch1.envelope.nrx2.init_vol;
        ch1.enabled = true;
        if (ch1.nrx1.length_load != 0)
            ch1.sequencer.length_timer = 64 - ch1.nrx1.length_load;
        ch1.envelope.enabled = true;
        // ch1.envelope.timer = ch1.envelope.nrx2.period;
    }

    void reset_ch2()
    {
        // Reset trigger
        ch2.frequency.nrx4.trigger = 0;
        mmu.write_byte(0xFF19, (byte)ch2.frequency.nrx4.value);

        ch2.enabled = true;

        if (ch2.sequencer.length_timer == 0)
            ch2.sequencer.length_timer = 64 - ch2.nrx1.length_load;

        ch2.envelope.volume = ch2.envelope.nrx2.init_vol;
        ch2.envelope.enabled = true;
    }

    void reset_ch3()
    {
        // Reset trigger
        wave.frequency.nrx4.trigger = 0;
        mmu.write_byte(0xFF1E, (byte)wave.frequency.nrx4.value);

        wave.enabled = true;
        wave.sample = 0;
        if (wave.sequencer.length_timer == 0)
            wave.sequencer.length_timer = 256 - wave.nr31.length_load;
    }

    void reset_ch4()
    {
        // Reset trigger
        noise.frequency.nrx4.trigger = 0;
        mmu.write_byte(0xFF23, (byte)noise.frequency.nrx4.value);

        if (noise.sequencer.length_timer == 0)
            noise.sequencer.length_timer = 64 - noise.nrx1.length_load;

        noise.enabled = true;

        noise.lfsr = 0x7fff;
        noise.envelope.volume = noise.envelope.nrx2.init_vol;
        noise.envelope.enabled = true;
    }

    int get_next_sample()
    {
        int ch1 = 0, ch2 = 0, ch3 = 0, ch4 = 0;

        for (int i = 0; i <= sample_rate; i++)
        {
            ch1 = get_ch1_sample();
            ch2 = get_ch2_sample();
            ch3 = get_ch3_sample();
            ch4 = get_ch4_sample();
        }

        if (status.isPaused || !status.soundEnabled)
            return 0x00;

        return ch1 + ch2 + ch3 + ch4;
    }


    void audio_callback(APU _sound, int[] _stream, int _length)
    {
        int[] stream = _stream;
        APU sound = _sound;
        int length = _length / stream[0];
        for (int i = 0; i < length; i += 2)
        {
            // TODO: Change to support stereo properly
            int sample = sound.get_next_sample();
            stream[i] = sample;
            stream[i + 1] = sample;
        }
    }
}





