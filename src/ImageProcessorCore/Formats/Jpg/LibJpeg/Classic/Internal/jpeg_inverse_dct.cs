/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

/*
 * This file contains the inverse-DCT management logic.
 * This code selects a particular IDCT implementation to be used,
 * and it performs related housekeeping chores.  No code in this file
 * is executed per IDCT step, only during output pass setup.
 *
 * Note that the IDCT routines are responsible for performing coefficient
 * dequantization as well as the IDCT proper.  This module sets up the
 * dequantization multiplier table needed by the IDCT routine.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace BitMiracle.LibJpeg.Classic.Internal
{
    /// <summary>
    /// An inverse DCT routine is given a pointer to the input JBLOCK and a pointer
    /// to an output sample array.  The routine must dequantize the input data as
    /// well as perform the IDCT; for dequantization, it uses the multiplier table
    /// pointed to by componentInfo.dct_table.  The output data is to be placed into the
    /// sample array starting at a specified column. (Any row offset needed will
    /// be applied to the array pointer before it is passed to the IDCT code)
    /// Note that the number of samples emitted by the IDCT routine is
    /// DCT_scaled_size * DCT_scaled_size.
    /// 
    /// Each IDCT routine has its own ideas about the best dct_table element type.
    /// 
    /// The decompressor input side saves away the appropriate
    /// quantization table for each component at the start of the first scan
    /// involving that component.  (This is necessary in order to correctly
    /// decode files that reuse Q-table slots.)
    /// When we are ready to make an output pass, the saved Q-table is converted
    /// to a multiplier table that will actually be used by the IDCT routine.
    /// The multiplier table contents are IDCT-method-dependent.  To support
    /// application changes in IDCT method between scans, we can remake the
    /// multiplier tables if necessary.
    /// In buffered-image mode, the first output pass may occur before any data
    /// has been seen for some components, and thus before their Q-tables have
    /// been saved away.  To handle this case, multiplier tables are preset
    /// to zeroes; the result of the IDCT will be a neutral gray level.
    /// </summary>
    class jpeg_inverse_dct
    {
        private const int IFAST_SCALE_BITS = 2; /* fractional bits in scale factors */

        /*
        * Each IDCT routine is responsible for range-limiting its results and
        * converting them to unsigned form (0..MAXJSAMPLE).  The raw outputs could
        * be quite far out of range if the input data is corrupt, so a bulletproof
        * range-limiting step is required.  We use a mask-and-table-lookup method
        * to do the combined operations quickly.  See the comments with
        * prepare_range_limit_table (in jdmaster.c) for more info.
        */
        private const int RANGE_MASK = (JpegConstants.MAXJSAMPLE * 4 + 3); /* 2 bits wider than legal samples */

        private const int SLOW_INTEGER_CONST_BITS = 13;
        private const int SLOW_INTEGER_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change SLOW_INTEGER_CONST_BITS you may want to add appropriate values.
        * 
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        * static int SLOW_INTEGER_FIX(double x)
        * {
        *  return ((int) ((x) * (((int) 1) << SLOW_INTEGER_CONST_BITS) + 0.5));
        * }
        */

        private const int SLOW_INTEGER_FIX_0_298631336 = 2446;   /* SLOW_INTEGER_FIX(0.298631336) */
        private const int SLOW_INTEGER_FIX_0_390180644 = 3196;   /* SLOW_INTEGER_FIX(0.390180644) */
        private const int SLOW_INTEGER_FIX_0_541196100 = 4433;   /* SLOW_INTEGER_FIX(0.541196100) */
        private const int SLOW_INTEGER_FIX_0_765366865 = 6270;   /* SLOW_INTEGER_FIX(0.765366865) */
        private const int SLOW_INTEGER_FIX_0_899976223 = 7373;   /* SLOW_INTEGER_FIX(0.899976223) */
        private const int SLOW_INTEGER_FIX_1_175875602 = 9633;   /* SLOW_INTEGER_FIX(1.175875602) */
        private const int SLOW_INTEGER_FIX_1_501321110 = 12299;  /* SLOW_INTEGER_FIX(1.501321110) */
        private const int SLOW_INTEGER_FIX_1_847759065 = 15137;  /* SLOW_INTEGER_FIX(1.847759065) */
        private const int SLOW_INTEGER_FIX_1_961570560 = 16069;  /* SLOW_INTEGER_FIX(1.961570560) */
        private const int SLOW_INTEGER_FIX_2_053119869 = 16819;  /* SLOW_INTEGER_FIX(2.053119869) */
        private const int SLOW_INTEGER_FIX_2_562915447 = 20995;  /* SLOW_INTEGER_FIX(2.562915447) */
        private const int SLOW_INTEGER_FIX_3_072711026 = 25172;  /* SLOW_INTEGER_FIX(3.072711026) */

        private const int FAST_INTEGER_CONST_BITS = 8;
        private const int FAST_INTEGER_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change FAST_INTEGER_CONST_BITS you may want to add appropriate values.
        */
        private const int FAST_INTEGER_FIX_1_082392200 = 277;        /* FAST_INTEGER_FIX(1.082392200) */
        private const int FAST_INTEGER_FIX_1_414213562 = 362;        /* FAST_INTEGER_FIX(1.414213562) */
        private const int FAST_INTEGER_FIX_1_847759065 = 473;        /* FAST_INTEGER_FIX(1.847759065) */
        private const int FAST_INTEGER_FIX_2_613125930 = 669;        /* FAST_INTEGER_FIX(2.613125930) */

        private const int REDUCED_CONST_BITS = 13;
        private const int REDUCED_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change REDUCED_CONST_BITS you may want to add appropriate values.
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        * static int REDUCED_FIX(double x)
        * {
        *   return ((int) ((x) * (((int) 1) << REDUCED_CONST_BITS) + 0.5));
        * }
        */

        private const int REDUCED_FIX_0_211164243 = 1730;    /* REDUCED_FIX(0.211164243) */
        private const int REDUCED_FIX_0_509795579 = 4176;    /* REDUCED_FIX(0.509795579) */
        private const int REDUCED_FIX_0_601344887 = 4926;    /* REDUCED_FIX(0.601344887) */
        private const int REDUCED_FIX_0_720959822 = 5906;    /* REDUCED_FIX(0.720959822) */
        private const int REDUCED_FIX_0_765366865 = 6270;    /* REDUCED_FIX(0.765366865) */
        private const int REDUCED_FIX_0_850430095 = 6967;    /* REDUCED_FIX(0.850430095) */
        private const int REDUCED_FIX_0_899976223 = 7373;    /* REDUCED_FIX(0.899976223) */
        private const int REDUCED_FIX_1_061594337 = 8697;    /* REDUCED_FIX(1.061594337) */
        private const int REDUCED_FIX_1_272758580 = 10426;   /* REDUCED_FIX(1.272758580) */
        private const int REDUCED_FIX_1_451774981 = 11893;   /* REDUCED_FIX(1.451774981) */
        private const int REDUCED_FIX_1_847759065 = 15137;   /* REDUCED_FIX(1.847759065) */
        private const int REDUCED_FIX_2_172734803 = 17799;   /* REDUCED_FIX(2.172734803) */
        private const int REDUCED_FIX_2_562915447 = 20995;   /* REDUCED_FIX(2.562915447) */
        private const int REDUCED_FIX_3_624509785 = 29692;   /* REDUCED_FIX(3.624509785) */

        /* precomputed values scaled up by 14 bits */
        private static short[] aanscales = 
        {
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 22725, 31521, 29692, 26722, 22725, 17855,
            12299, 6270, 21407, 29692, 27969, 25172, 21407, 16819, 11585,
            5906, 19266, 26722, 25172, 22654, 19266, 15137, 10426, 5315,
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 12873,
            17855, 16819, 15137, 12873, 10114, 6967, 3552, 8867, 12299,
            11585, 10426, 8867, 6967, 4799, 2446, 4520, 6270, 5906, 5315,
            4520, 3552, 2446, 1247 
        };

        private const int CONST_BITS = 14;

        private static double[] aanscalefactor = 
        { 
            1.0, 1.387039845, 1.306562965, 1.175875602, 1.0,
            0.785694958, 0.541196100, 0.275899379 
        };

        private enum InverseMethod
        {
            Unknown,
            idct_1x1_method,
            idct_2x2_method,
            idct_4x4_method,
            idct_islow_method,
            idct_ifast_method,
            idct_float_method
        }

        /* It is useful to allow each component to have a separate IDCT method. */
        private InverseMethod[] m_inverse_DCT_method = new InverseMethod[JpegConstants.MAX_COMPONENTS];

        /* Allocated multiplier tables: big enough for any supported variant */
        private class multiplier_table
        {
            public int[] int_array = new int[JpegConstants.DCTSIZE2];
            public float[] float_array = new float[JpegConstants.DCTSIZE2];
        };

        private multiplier_table[] m_dctTables;

        private jpeg_decompress_struct m_cinfo;

        /* This array contains the IDCT method code that each multiplier table
        * is currently set up for, or -1 if it's not yet set up.
        * The actual multiplier tables are pointed to by dct_table in the
        * per-component comp_info structures.
        */
        private int[] m_cur_method = new int[JpegConstants.MAX_COMPONENTS];

        private ComponentBuffer m_componentBuffer;

        public jpeg_inverse_dct(jpeg_decompress_struct cinfo)
        {
            m_cinfo = cinfo;

            m_dctTables = new multiplier_table[cinfo.m_num_components];
            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                /* Allocate and pre-zero a multiplier table for each component */
                m_dctTables[ci] = new multiplier_table();

                /* Mark multiplier table not yet set up for any method */
                m_cur_method[ci] = -1;
            }
        }

        /// <summary>
        /// Prepare for an output pass.
        /// Here we select the proper IDCT routine for each component and build
        /// a matching multiplier table.
        /// </summary>
        public void start_pass()
        {
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                jpeg_component_info componentInfo = m_cinfo.Comp_info[ci];

                InverseMethod im = InverseMethod.Unknown;
                int method = 0;
                /* Select the proper IDCT routine for this component's scaling */
                switch (componentInfo.DCT_scaled_size)
                {
                    case 1:
                        im = InverseMethod.idct_1x1_method;
                        method = (int)J_DCT_METHOD.JDCT_ISLOW;    /* jidctred uses islow-style table */
                        break;
                    case 2:
                        im = InverseMethod.idct_2x2_method;
                        method = (int)J_DCT_METHOD.JDCT_ISLOW;    /* jidctred uses islow-style table */
                        break;
                    case 4:
                        im = InverseMethod.idct_4x4_method;
                        method = (int)J_DCT_METHOD.JDCT_ISLOW;    /* jidctred uses islow-style table */
                        break;
                    case JpegConstants.DCTSIZE:
                        switch (m_cinfo.m_dct_method)
                        {
                            case J_DCT_METHOD.JDCT_ISLOW:
                                im = InverseMethod.idct_islow_method;
                                method = (int)J_DCT_METHOD.JDCT_ISLOW;
                                break;
                            case J_DCT_METHOD.JDCT_IFAST:
                                im = InverseMethod.idct_ifast_method;
                                method = (int)J_DCT_METHOD.JDCT_IFAST;
                                break;
                            case J_DCT_METHOD.JDCT_FLOAT:
                                im = InverseMethod.idct_float_method;
                                method = (int)J_DCT_METHOD.JDCT_FLOAT;
                                break;
                            default:
                                m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
                                break;
                        }
                        break;
                    default:
                        m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_BAD_DCTSIZE, componentInfo.DCT_scaled_size);
                        break;
                }

                m_inverse_DCT_method[ci] = im;

                /* Create multiplier table from quant table.
                 * However, we can skip this if the component is uninteresting
                 * or if we already built the table.  Also, if no quant table
                 * has yet been saved for the component, we leave the
                 * multiplier table all-zero; we'll be reading zeroes from the
                 * coefficient controller's buffer anyway.
                 */
                if (!componentInfo.component_needed || m_cur_method[ci] == method)
                    continue;

                if (componentInfo.quant_table == null)
                {
                    /* happens if no data yet for component */
                    continue;
                }
                
                m_cur_method[ci] = method;
                switch ((J_DCT_METHOD)method)
                {
                    case J_DCT_METHOD.JDCT_ISLOW:
                        /* For LL&M IDCT method, multipliers are equal to raw quantization
                         * coefficients, but are stored as ints to ensure access efficiency.
                         */
                        int[] ismtbl = m_dctTables[ci].int_array;
                        for (int i = 0; i < JpegConstants.DCTSIZE2; i++)
                            ismtbl[i] = componentInfo.quant_table.quantval[i];
                        break;

                    case J_DCT_METHOD.JDCT_IFAST:
                        /* For AA&N IDCT method, multipliers are equal to quantization
                         * coefficients scaled by scalefactor[row]*scalefactor[col], where
                         *   scalefactor[0] = 1
                         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
                         * For integer operation, the multiplier table is to be scaled by
                         * IFAST_SCALE_BITS.
                         */
                        int[] ifmtbl = m_dctTables[ci].int_array;
                        
                        for (int i = 0; i < JpegConstants.DCTSIZE2; i++)
                        {
                            ifmtbl[i] = JpegUtils.DESCALE((int)componentInfo.quant_table.quantval[i] * (int)aanscales[i], CONST_BITS - IFAST_SCALE_BITS);
                        }
                        break;

                    case J_DCT_METHOD.JDCT_FLOAT:
                        /* For float AA&N IDCT method, multipliers are equal to quantization
                         * coefficients scaled by scalefactor[row]*scalefactor[col], where
                         *   scalefactor[0] = 1
                         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
                         */
                        float[] fmtbl = m_dctTables[ci].float_array;
                        int ii = 0;
                        for (int row = 0; row < JpegConstants.DCTSIZE; row++)
                        {
                            for (int col = 0; col < JpegConstants.DCTSIZE; col++)
                            {
                                fmtbl[ii] = (float) ((double) componentInfo.quant_table.quantval[ii] * aanscalefactor[row] * aanscalefactor[col]);
                                ii++;
                            }
                        }
                        break;

                    default:
                        m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
                        break;
                }
            }
        }

        /* Inverse DCT (also performs dequantization) */
        public void inverse(int component_index, short[] coef_block, ComponentBuffer output_buf, int output_row, int output_col)
        {
            m_componentBuffer = output_buf;
            switch (m_inverse_DCT_method[component_index])
            {
                case InverseMethod.idct_1x1_method:
                    jpeg_idct_1x1(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_2x2_method:
                    jpeg_idct_2x2(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_4x4_method:
                    jpeg_idct_4x4(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_islow_method:
                    jpeg_idct_islow(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_ifast_method:
                    jpeg_idct_ifast(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_float_method:
                    jpeg_idct_float(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.Unknown:
                default:
                    m_cinfo.ERREXIT(J_MESSAGE_CODE.JERR_NOT_COMPILED);
                    break;
            }
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// A slow-but-accurate integer implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform dequantization of the input coefficients.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on an algorithm described in
        /// C. Loeffler, A. Ligtenberg and G. Moschytz, "Practical Fast 1-D DCT
        /// Algorithms with 11 Multiplications", Proc. Int'l. Conf. on Acoustics,
        /// Speech, and Signal Processing 1989 (ICASSP '89), pp. 988-991.
        /// The primary algorithm described there uses 11 multiplies and 29 adds.
        /// We use their alternate method with 12 multiplies and 32 adds.
        /// The advantage of this method is that no data path contains more than one
        /// multiplication; this allows a very simple and accurate implementation in
        /// scaled fixed-point arithmetic, with a minimal number of shifts.
        /// 
        /// The poop on this scaling stuff is as follows:
        /// 
        /// Each 1-D IDCT step produces outputs which are a factor of sqrt(N)
        /// larger than the true IDCT outputs.  The final outputs are therefore
        /// a factor of N larger than desired; since N=8 this can be cured by
        /// a simple right shift at the end of the algorithm.  The advantage of
        /// this arrangement is that we save two multiplications per 1-D IDCT,
        /// because the y0 and y4 inputs need not be divided by sqrt(N).
        /// 
        /// We have to do addition and subtraction of the integer inputs, which
        /// is no problem, and multiplication by fractional constants, which is
        /// a problem to do in integer arithmetic.  We multiply all the constants
        /// by CONST_SCALE and convert them to integer constants (thus retaining
        /// SLOW_INTEGER_CONST_BITS bits of precision in the constants).  After doing a
        /// multiplication we have to divide the product by CONST_SCALE, with proper
        /// rounding, to produce the correct output.  This division can be done
        /// cheaply as a right shift of SLOW_INTEGER_CONST_BITS bits.  We postpone shifting
        /// as long as possible so that partial sums can be added together with
        /// full fractional precision.
        /// 
        /// The outputs of the first pass are scaled up by SLOW_INTEGER_PASS1_BITS bits so that
        /// they are represented to better-than-integral precision.  These outputs
        /// require BITS_IN_JSAMPLE + SLOW_INTEGER_PASS1_BITS + 3 bits; this fits in a 16-bit word
        /// with the recommended scaling.  (To scale up 12-bit sample data further, an
        /// intermediate int array would be needed.)
        /// 
        /// To avoid overflow of the 32-bit intermediate results in pass 2, we must
        /// have BITS_IN_JSAMPLE + SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS &lt;= 26.  Error analysis
        /// shows that the values given below are the most effective.
        /// </summary>
        private void jpeg_idct_islow(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSIZE2];

            /* Pass 1: process columns from input, store into work array. */
            /* Note results are scaled up by sqrt(8) compared to a true IDCT; */
            /* furthermore, we scale the results by 2**SLOW_INTEGER_PASS1_BITS. */

            int coefBlockIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;
            
            int workspaceIndex = 0;

            for (int ctr = JpegConstants.DCTSIZE; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7] == 0)
                {
                    /* AC terms all zero */
                    int dcval = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]) << SLOW_INTEGER_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = dcval;

                    /* advance pointers to next column */
                    coefBlockIndex++;
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part: reverse the even part of the forward DCT. */
                /* The rotator is sqrt(2)*c(-6). */

                int z2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 2]);
                int z3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 6]);

                int z1 = (z2 + z3) * SLOW_INTEGER_FIX_0_541196100;
                int tmp2 = z1 + z3 * (-SLOW_INTEGER_FIX_1_847759065);
                int tmp3 = z1 + z2 * SLOW_INTEGER_FIX_0_765366865;

                z2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);
                z3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 4]);

                int tmp0 = (z2 + z3) << SLOW_INTEGER_CONST_BITS;
                int tmp1 = (z2 - z3) << SLOW_INTEGER_CONST_BITS;

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                /* Odd part per figure 8; the matrix is unitary and hence its
                * transpose is its inverse.  i0..i3 are y7,y5,y3,y1 respectively.
                */

                tmp0 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 7]);
                tmp1 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 5]);
                tmp2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 3]);
                tmp3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 1]);

                z1 = tmp0 + tmp3;
                z2 = tmp1 + tmp2;
                z3 = tmp0 + tmp2;
                int z4 = tmp1 + tmp3;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp0 = tmp0 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp1 = tmp1 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp2 = tmp2 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp3 = tmp3 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                tmp0 += z1 + z3;
                tmp1 += z2 + z4;
                tmp2 += z2 + z3;
                tmp3 += z1 + z4;

                /* Final output stage: inputs are tmp10..tmp13, tmp0..tmp3 */

                workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = JpegUtils.DESCALE(tmp10 + tmp3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = JpegUtils.DESCALE(tmp10 - tmp3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = JpegUtils.DESCALE(tmp11 + tmp2, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = JpegUtils.DESCALE(tmp11 - tmp2, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = JpegUtils.DESCALE(tmp12 + tmp1, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = JpegUtils.DESCALE(tmp12 - tmp1, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = JpegUtils.DESCALE(tmp13 + tmp0, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = JpegUtils.DESCALE(tmp13 - tmp0, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);

                /* advance pointers to next column */
                coefBlockIndex++;
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3, */
            /* and also undo the SLOW_INTEGER_PASS1_BITS scaling. */

            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            for (int ctr = 0; ctr < JpegConstants.DCTSIZE; ctr++)
            {
                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * On machines with very fast multiplication, it's possible that the
                * test takes more time than it's worth.  In that case this section
                * may be commented out.
                */
                int currentOutRow = output_row + ctr;
                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 2] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 4] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 6] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 4] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 5] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 6] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 7] = dcval;

                    workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
                    continue;
                }

                /* Even part: reverse the even part of the forward DCT. */
                /* The rotator is sqrt(2)*c(-6). */

                int z2 = workspace[workspaceIndex + 2];
                int z3 = workspace[workspaceIndex + 6];

                int z1 = (z2 + z3) * SLOW_INTEGER_FIX_0_541196100;
                int tmp2 = z1 + z3 * (-SLOW_INTEGER_FIX_1_847759065);
                int tmp3 = z1 + z2 * SLOW_INTEGER_FIX_0_765366865;

                int tmp0 = (workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4]) << SLOW_INTEGER_CONST_BITS;
                int tmp1 = (workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4]) << SLOW_INTEGER_CONST_BITS;

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                /* Odd part per figure 8; the matrix is unitary and hence its
                * transpose is its inverse.  i0..i3 are y7,y5,y3,y1 respectively.
                */

                tmp0 = workspace[workspaceIndex + 7];
                tmp1 = workspace[workspaceIndex + 5];
                tmp2 = workspace[workspaceIndex + 3];
                tmp3 = workspace[workspaceIndex + 1];

                z1 = tmp0 + tmp3;
                z2 = tmp1 + tmp2;
                z3 = tmp0 + tmp2;
                int z4 = tmp1 + tmp3;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp0 = tmp0 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp1 = tmp1 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp2 = tmp2 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp3 = tmp3 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                tmp0 += z1 + z3;
                tmp1 += z2 + z4;
                tmp2 += z2 + z3;
                tmp3 += z1 + z4;

                /* Final output stage: inputs are tmp10..tmp13, tmp0..tmp3 */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp11 + tmp2, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + JpegUtils.DESCALE(tmp11 - tmp2, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE(tmp12 + tmp1, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + JpegUtils.DESCALE(tmp12 - tmp1, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE(tmp13 + tmp0, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + JpegUtils.DESCALE(tmp13 - tmp0, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                /* advance pointer to next row */
                workspaceIndex += JpegConstants.DCTSIZE;
            }
        }
        
        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce an int result.  In this module, both inputs and result
        /// are 16 bits or less, so either int or short multiply will work.
        /// </summary>
        private static int SLOW_INTEGER_DEQUANTIZE(int coef, int quantval)
        {
            return (coef * quantval);
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A fast, not so accurate integer implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform dequantization of the input coefficients.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with fixed-point math,
        /// accuracy is lost due to imprecise representation of the scaled
        /// quantization values.  The smaller the quantization table entry, the less
        /// precise the scaled value, so this implementation does worse with high-
        /// quality-setting files than with low-quality ones.
        /// 
        /// Scaling decisions are generally the same as in the LL&amp;M algorithm;
        /// However, we choose to descale
        /// (right shift) multiplication products as soon as they are formed,
        /// rather than carrying additional fractional bits into subsequent additions.
        /// This compromises accuracy slightly, but it lets us save a few shifts.
        /// More importantly, 16-bit arithmetic is then adequate (for 8-bit samples)
        /// everywhere except in the multiplications proper; this saves a good deal
        /// of work on 16-bit-int machines.
        /// 
        /// The dequantized coefficients are not integers because the AA&amp;N scaling
        /// factors have been incorporated.  We represent them scaled up by FAST_INTEGER_PASS1_BITS,
        /// so that the first and second IDCT rounds have the same input scaling.
        /// For 8-bit JSAMPLEs, we choose IFAST_SCALE_BITS = FAST_INTEGER_PASS1_BITS so as to
        /// avoid a descaling shift; this compromises accuracy rather drastically
        /// for small quantization table entries, but it saves a lot of shifts.
        /// For 12-bit JSAMPLEs, there's no hope of using 16x16 multiplies anyway,
        /// so we use a much larger scaling factor to preserve accuracy.
        /// 
        /// A final compromise is to represent the multiplicative constants to only
        /// 8 fractional bits, rather than 13.  This saves some shifting work on some
        /// machines, and may also reduce the cost of multiplication (since there
        /// are fewer one-bits in the constants).
        /// </summary>
        private void jpeg_idct_ifast(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSIZE2];

            /* Pass 1: process columns from input, store into work array. */

            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSIZE; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7] == 0)
                {
                    /* AC terms all zero */
                    int dcval = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                        quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);

                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = dcval;

                    /* advance pointers to next column */
                    coefBlockIndex++;
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part */

                int tmp0 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);
                int tmp1 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 2]);
                int tmp2 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 4]);
                int tmp3 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 6]);

                int tmp10 = tmp0 + tmp2;    /* phase 3 */
                int tmp11 = tmp0 - tmp2;

                int tmp13 = tmp1 + tmp3;    /* phases 5-3 */
                int tmp12 = FAST_INTEGER_MULTIPLY(tmp1 - tmp3, FAST_INTEGER_FIX_1_414213562) - tmp13; /* 2*c4 */

                tmp0 = tmp10 + tmp13;   /* phase 2 */
                tmp3 = tmp10 - tmp13;
                tmp1 = tmp11 + tmp12;
                tmp2 = tmp11 - tmp12;

                /* Odd part */

                int tmp4 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 1]);
                int tmp5 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 3]);
                int tmp6 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 5]);
                int tmp7 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 7]);

                int z13 = tmp6 + tmp5;      /* phase 6 */
                int z10 = tmp6 - tmp5;
                int z11 = tmp4 + tmp7;
                int z12 = tmp4 - tmp7;

                tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = FAST_INTEGER_MULTIPLY(z11 - z13, FAST_INTEGER_FIX_1_414213562); /* 2*c4 */

                int z5 = FAST_INTEGER_MULTIPLY(z10 + z12, FAST_INTEGER_FIX_1_847759065); /* 2*c2 */
                tmp10 = FAST_INTEGER_MULTIPLY(z12, FAST_INTEGER_FIX_1_082392200) - z5; /* 2*(c2-c6) */
                tmp12 = FAST_INTEGER_MULTIPLY(z10, -FAST_INTEGER_FIX_2_613125930) + z5; /* -2*(c2+c6) */

                tmp6 = tmp12 - tmp7;    /* phase 2 */
                tmp5 = tmp11 - tmp6;
                tmp4 = tmp10 + tmp5;

                workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = tmp0 + tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = tmp0 - tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = tmp1 + tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = tmp1 - tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = tmp2 + tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = tmp2 - tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = tmp3 + tmp4;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = tmp3 - tmp4;

                /* advance pointers to next column */
                coefBlockIndex++;
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3, */
            /* and also undo the FAST_INTEGER_PASS1_BITS scaling. */

            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            for (int ctr = 0; ctr < JpegConstants.DCTSIZE; ctr++)
            {
                int currentOutRow = output_row + ctr;

                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * On machines with very fast multiplication, it's possible that the
                * test takes more time than it's worth.  In that case this section
                * may be commented out.
                */

                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 2] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 4] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 6] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + FAST_INTEGER_IDESCALE(workspace[workspaceIndex + 0], FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 4] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 5] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 6] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 7] = dcval;

                    /* advance pointer to next row */
                    workspaceIndex += JpegConstants.DCTSIZE;
                    continue;
                }

                /* Even part */

                int tmp10 = workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4];
                int tmp11 = workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4];

                int tmp13 = workspace[workspaceIndex + 2] + workspace[workspaceIndex + 6];
                int tmp12 = FAST_INTEGER_MULTIPLY(workspace[workspaceIndex + 2] - workspace[workspaceIndex + 6], FAST_INTEGER_FIX_1_414213562) - tmp13;

                int tmp0 = tmp10 + tmp13;
                int tmp3 = tmp10 - tmp13;
                int tmp1 = tmp11 + tmp12;
                int tmp2 = tmp11 - tmp12;

                /* Odd part */

                int z13 = workspace[workspaceIndex + 5] + workspace[workspaceIndex + 3];
                int z10 = workspace[workspaceIndex + 5] - workspace[workspaceIndex + 3];
                int z11 = workspace[workspaceIndex + 1] + workspace[workspaceIndex + 7];
                int z12 = workspace[workspaceIndex + 1] - workspace[workspaceIndex + 7];

                int tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = FAST_INTEGER_MULTIPLY(z11 - z13, FAST_INTEGER_FIX_1_414213562); /* 2*c4 */

                int z5 = FAST_INTEGER_MULTIPLY(z10 + z12, FAST_INTEGER_FIX_1_847759065); /* 2*c2 */
                tmp10 = FAST_INTEGER_MULTIPLY(z12, FAST_INTEGER_FIX_1_082392200) - z5; /* 2*(c2-c6) */
                tmp12 = FAST_INTEGER_MULTIPLY(z10, -FAST_INTEGER_FIX_2_613125930) + z5; /* -2*(c2+c6) */

                int tmp6 = tmp12 - tmp7;    /* phase 2 */
                int tmp5 = tmp11 - tmp6;
                int tmp4 = tmp10 + tmp5;

                /* Final output stage: scale down by a factor of 8 and range-limit */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp0 + tmp7, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp0 - tmp7, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp1 + tmp6, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp1 - tmp6, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp2 + tmp5, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp2 - tmp5, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp3 + tmp4, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp3 - tmp4, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                /* advance pointer to next row */
                workspaceIndex += JpegConstants.DCTSIZE;
            }
        }

        /// <summary>
        /// Multiply a DCTELEM variable by an int constant, and immediately
        /// descale to yield a DCTELEM result.
        /// </summary>
        private static int FAST_INTEGER_MULTIPLY(int var, int c)
        {
#if !USE_ACCURATE_ROUNDING
            return (JpegUtils.RIGHT_SHIFT(var * c, FAST_INTEGER_CONST_BITS));
#else
            return (JpegUtils.DESCALE(var * c, FAST_INTEGER_CONST_BITS));
#endif
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce a DCTELEM result.  For 8-bit data a 16x16->16
        /// multiplication will do.  For 12-bit data, the multiplier table is
        /// declared int, so a 32-bit multiply will be used.
        /// </summary>
        private static int FAST_INTEGER_DEQUANTIZE(short coef, int quantval)
        {
            return ((int)coef * quantval);
        }

        /// <summary>
        /// Like DESCALE, but applies to a DCTELEM and produces an int.
        /// We assume that int right shift is unsigned if int right shift is.
        /// </summary>
        private static int FAST_INTEGER_IRIGHT_SHIFT(int x, int shft)
        {
            return (x >> shft);
        }

        private static int FAST_INTEGER_IDESCALE(int x, int n)
        {
#if USE_ACCURATE_ROUNDING
            return (FAST_INTEGER_IRIGHT_SHIFT((x) + (1 << ((n) - 1)), n));
#else
            return (FAST_INTEGER_IRIGHT_SHIFT(x, n));
#endif
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A floating-point implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform dequantization of the input coefficients.
        /// 
        /// This implementation should be more accurate than either of the integer
        /// IDCT implementations.  However, it may not give the same results on all
        /// machines because of differences in roundoff behavior.  Speed will depend
        /// on the hardware's floating point capacity.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with a fixed-point
        /// implementation, accuracy is lost due to imprecise representation of the
        /// scaled quantization values.  However, that problem does not arise if
        /// we use floating point arithmetic.
        /// </summary>
        private void jpeg_idct_float(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            float[] workspace = new float[JpegConstants.DCTSIZE2];

            /* Pass 1: process columns from input, store into work array. */

            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            float[] quantTable = m_dctTables[component_index].float_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSIZE; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7] == 0)
                {
                    /* AC terms all zero */
                    float dcval = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                        quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);

                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = dcval;

                    coefBlockIndex++;            /* advance pointers to next column */
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part */

                float tmp0 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);
                float tmp1 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 2]);
                float tmp2 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 4], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 4]);
                float tmp3 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 6]);

                float tmp10 = tmp0 + tmp2;    /* phase 3 */
                float tmp11 = tmp0 - tmp2;

                float tmp13 = tmp1 + tmp3;    /* phases 5-3 */
                float tmp12 = (tmp1 - tmp3) * 1.414213562f - tmp13; /* 2*c4 */

                tmp0 = tmp10 + tmp13;   /* phase 2 */
                tmp3 = tmp10 - tmp13;
                tmp1 = tmp11 + tmp12;
                tmp2 = tmp11 - tmp12;

                /* Odd part */

                float tmp4 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 1]);
                float tmp5 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 3]);
                float tmp6 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 5]);
                float tmp7 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 7]);

                float z13 = tmp6 + tmp5;      /* phase 6 */
                float z10 = tmp6 - tmp5;
                float z11 = tmp4 + tmp7;
                float z12 = tmp4 - tmp7;

                tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = (z11 - z13) * 1.414213562f; /* 2*c4 */

                float z5 = (z10 + z12) * 1.847759065f; /* 2*c2 */
                tmp10 = 1.082392200f * z12 - z5; /* 2*(c2-c6) */
                tmp12 = -2.613125930f * z10 + z5; /* -2*(c2+c6) */

                tmp6 = tmp12 - tmp7;    /* phase 2 */
                tmp5 = tmp11 - tmp6;
                tmp4 = tmp10 + tmp5;

                workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = tmp0 + tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 7] = tmp0 - tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = tmp1 + tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 6] = tmp1 - tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = tmp2 + tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 5] = tmp2 - tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 4] = tmp3 + tmp4;
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = tmp3 - tmp4;

                coefBlockIndex++;            /* advance pointers to next column */
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3. */
            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            for (int ctr = 0; ctr < JpegConstants.DCTSIZE; ctr++)
            {
                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * And testing floats for zero is relatively expensive, so we don't bother.
                */

                /* Even part */

                float tmp10 = workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4];
                float tmp11 = workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4];

                float tmp13 = workspace[workspaceIndex + 2] + workspace[workspaceIndex + 6];
                float tmp12 = (workspace[workspaceIndex + 2] - workspace[workspaceIndex + 6]) * 1.414213562f - tmp13;

                float tmp0 = tmp10 + tmp13;
                float tmp3 = tmp10 - tmp13;
                float tmp1 = tmp11 + tmp12;
                float tmp2 = tmp11 - tmp12;

                /* Odd part */

                float z13 = workspace[workspaceIndex + 5] + workspace[workspaceIndex + 3];
                float z10 = workspace[workspaceIndex + 5] - workspace[workspaceIndex + 3];
                float z11 = workspace[workspaceIndex + 1] + workspace[workspaceIndex + 7];
                float z12 = workspace[workspaceIndex + 1] - workspace[workspaceIndex + 7];

                float tmp7 = z11 + z13;
                tmp11 = (z11 - z13) * 1.414213562f;

                float z5 = (z10 + z12) * 1.847759065f; /* 2*c2 */
                tmp10 = 1.082392200f * z12 - z5; /* 2*(c2-c6) */
                tmp12 = -2.613125930f * z10 + z5; /* -2*(c2+c6) */

                float tmp6 = tmp12 - tmp7;
                float tmp5 = tmp11 - tmp6;
                float tmp4 = tmp10 + tmp5;

                /* Final output stage: scale down by a factor of 8 and range-limit */
                int currentOutRow = output_row + ctr;
                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp0 + tmp7), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp0 - tmp7), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp1 + tmp6), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp1 - tmp6), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp2 + tmp5), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp2 - tmp5), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp3 + tmp4), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp3 - tmp4), 3) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce a float result.
        /// </summary>
        private static float FLOAT_DEQUANTIZE(short coef, float quantval)
        {
            return (((float)(coef)) * (quantval));
        }

        /// <summary>
        /// Inverse-DCT routines that produce reduced-size output:
        /// either 4x4, 2x2, or 1x1 pixels from an 8x8 DCT block.
        /// 
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// The implementation is based on the Loeffler, Ligtenberg and Moschytz (LL&amp;M)
        /// algorithm. We simply replace each 8-to-8 1-D IDCT step
        /// with an 8-to-4 step that produces the four averages of two adjacent outputs
        /// (or an 8-to-2 step producing two averages of four outputs, for 2x2 output).
        /// These steps were derived by computing the corresponding values at the end
        /// of the normal LL&amp;M code, then simplifying as much as possible.
        /// 
        /// 1x1 is trivial: just take the DC coefficient divided by 8.
        /// 
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 4x4 output block.
        /// </summary>
        private void jpeg_idct_4x4(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSIZE * 4];

            /* Pass 1: process columns from input, store into work array. */
            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSIZE; ctr > 0; coefBlockIndex++, quantTableIndex++, workspaceIndex++, ctr--)
            {
                /* Don't bother to process column 4, because second pass won't use it */
                if (ctr == JpegConstants.DCTSIZE - 4)
                    continue;

                if (coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7] == 0)
                {
                    /* AC terms all zero; we need not examine term 4 for 4x4 output */
                    int dcval = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                        quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]) << REDUCED_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = dcval;

                    continue;
                }

                /* Even part */

                int tmp0 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);
                tmp0 <<= (REDUCED_CONST_BITS + 1);

                int z2 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 2], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 2]);
                int z3 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 6], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 6]);

                int tmp2 = z2 * REDUCED_FIX_1_847759065 + z3 * (-REDUCED_FIX_0_765366865);

                int tmp10 = tmp0 + tmp2;
                int tmp12 = tmp0 - tmp2;

                /* Odd part */

                int z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 7]);
                z2 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 5]);
                z3 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 3]);
                int z4 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 1]);

                tmp0 = z1 * (-REDUCED_FIX_0_211164243) /* sqrt(2) * (c3-c1) */ +
                       z2 * REDUCED_FIX_1_451774981 /* sqrt(2) * (c3+c7) */ +
                       z3 * (-REDUCED_FIX_2_172734803) /* sqrt(2) * (-c1-c5) */ +
                       z4 * REDUCED_FIX_1_061594337; /* sqrt(2) * (c5+c7) */

                tmp2 = z1 * (-REDUCED_FIX_0_509795579) /* sqrt(2) * (c7-c5) */ +
                       z2 * (-REDUCED_FIX_0_601344887) /* sqrt(2) * (c5-c1) */ +
                       z3 * REDUCED_FIX_0_899976223 /* sqrt(2) * (c3-c7) */ +
                       z4 * REDUCED_FIX_2_562915447; /* sqrt(2) * (c1+c3) */

                /* Final output stage */

                workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = JpegUtils.DESCALE(tmp10 + tmp2, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 3] = JpegUtils.DESCALE(tmp10 - tmp2, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = JpegUtils.DESCALE(tmp12 + tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 2] = JpegUtils.DESCALE(tmp12 - tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
            }

            /* Pass 2: process 4 rows from work array, store into output array. */
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            workspaceIndex = 0;
            for (int ctr = 0; ctr < 4; ctr++)
            {
                int currentOutRow = output_row + ctr;
                /* It's not clear whether a zero row test is worthwhile here ... */

                if (workspace[workspaceIndex + 1] == 0 && 
                    workspace[workspaceIndex + 2] == 0 && 
                    workspace[workspaceIndex + 3] == 0 && 
                    workspace[workspaceIndex + 5] == 0 && 
                    workspace[workspaceIndex + 6] == 0 && 
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], REDUCED_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;

                    workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
                    continue;
                }

                /* Even part */

                int tmp0 = (workspace[workspaceIndex + 0]) << (REDUCED_CONST_BITS + 1);

                int tmp2 = workspace[workspaceIndex + 2] * REDUCED_FIX_1_847759065 + workspace[workspaceIndex + 6] * (-REDUCED_FIX_0_765366865);

                int tmp10 = tmp0 + tmp2;
                int tmp12 = tmp0 - tmp2;

                /* Odd part */

                int z1 = workspace[workspaceIndex + 7];
                int z2 = workspace[workspaceIndex + 5];
                int z3 = workspace[workspaceIndex + 3];
                int z4 = workspace[workspaceIndex + 1];

                tmp0 = z1 * (-REDUCED_FIX_0_211164243) /* sqrt(2) * (c3-c1) */ +
                       z2 * REDUCED_FIX_1_451774981 /* sqrt(2) * (c3+c7) */ +
                       z3 * (-REDUCED_FIX_2_172734803) /* sqrt(2) * (-c1-c5) */ +
                       z4 * REDUCED_FIX_1_061594337; /* sqrt(2) * (c5+c7) */

                tmp2 = z1 * (-REDUCED_FIX_0_509795579) /* sqrt(2) * (c7-c5) */ +
                       z2 * (-REDUCED_FIX_0_601344887) /* sqrt(2) * (c5-c1) */ +
                       z3 * REDUCED_FIX_0_899976223 /* sqrt(2) * (c3-c7) */ +
                       z4 * REDUCED_FIX_2_562915447; /* sqrt(2) * (c1+c3) */

                /* Final output stage */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp2, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp2, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp12 + tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE(tmp12 - tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 2x2 output block.
        /// </summary>
        private void jpeg_idct_2x2(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSIZE * 2];

            /* Pass 1: process columns from input, store into work array. */
            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSIZE; ctr > 0; coefBlockIndex++, quantTableIndex++, workspaceIndex++, ctr--)
            {
                /* Don't bother to process columns 2,4,6 */
                if (ctr == JpegConstants.DCTSIZE - 2 || ctr == JpegConstants.DCTSIZE - 4 || ctr == JpegConstants.DCTSIZE - 6)
                    continue;

                if (coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1] == 0 && 
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3] == 0 && 
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5] == 0 && 
                    coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7] == 0)
                {
                    /* AC terms all zero; we need not examine terms 2,4,6 for 2x2 output */
                    int dcval = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                        quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]) << REDUCED_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = dcval;

                    continue;
                }

                /* Even part */

                int z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 0], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 0]);
                int tmp10 = z1 << (REDUCED_CONST_BITS + 2);

                /* Odd part */

                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 7], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 7]);
                int tmp0 = z1 * -REDUCED_FIX_0_720959822; /* sqrt(2) * (c7-c5+c3-c1) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 5], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 5]);
                tmp0 += z1 * REDUCED_FIX_0_850430095; /* sqrt(2) * (-c1+c3+c5+c7) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 3], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 3]);
                tmp0 += z1 * (-REDUCED_FIX_1_272758580); /* sqrt(2) * (-c1+c3-c5-c7) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSIZE * 1], 
                    quantTable[quantTableIndex + JpegConstants.DCTSIZE * 1]);
                tmp0 += z1 * REDUCED_FIX_3_624509785; /* sqrt(2) * (c1+c3+c5+c7) */

                /* Final output stage */

                workspace[workspaceIndex + JpegConstants.DCTSIZE * 0] = JpegUtils.DESCALE(tmp10 + tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 2);
                workspace[workspaceIndex + JpegConstants.DCTSIZE * 1] = JpegUtils.DESCALE(tmp10 - tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 2);
            }

            /* Pass 2: process 2 rows from work array, store into output array. */
            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            for (int ctr = 0; ctr < 2; ctr++)
            {
                int currentOutRow = output_row + ctr;
                /* It's not clear whether a zero row test is worthwhile here ... */

                if (workspace[workspaceIndex + 1] == 0 && 
                    workspace[workspaceIndex + 3] == 0 && 
                    workspace[workspaceIndex + 5] == 0 && 
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], REDUCED_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;

                    workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
                    continue;
                }

                /* Even part */

                int tmp10 = (workspace[workspaceIndex + 0]) << (REDUCED_CONST_BITS + 2);

                /* Odd part */

                int tmp0 = workspace[workspaceIndex + 7] * (-REDUCED_FIX_0_720959822) /* sqrt(2) * (c7-c5+c3-c1) */ +
                       workspace[workspaceIndex + 5] * REDUCED_FIX_0_850430095 /* sqrt(2) * (-c1+c3+c5+c7) */ +
                       workspace[workspaceIndex + 3] * (-REDUCED_FIX_1_272758580) /* sqrt(2) * (-c1+c3-c5-c7) */ +
                       workspace[workspaceIndex + 1] * REDUCED_FIX_3_624509785; /* sqrt(2) * (c1+c3+c5+c7) */

                /* Final output stage */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 2) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 2) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSIZE;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 1x1 output block.
        /// </summary>
        private void jpeg_idct_1x1(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* We hardly need an inverse DCT routine for this: just take the
            * average pixel value, which is one-eighth of the DC coefficient.
            */
            int[] quantptr = m_dctTables[component_index].int_array;
            int dcval = REDUCED_DEQUANTIZE(coef_block[0], quantptr[0]);
            dcval = JpegUtils.DESCALE(dcval, 3);

            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.CENTERJSAMPLE;

            m_componentBuffer[output_row + 0][output_col] = limit[limitOffset + dcval & RANGE_MASK];
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce an int result.  In this module, both inputs and result
        /// are 16 bits or less, so either int or short multiply will work.
        /// </summary>
        private static int REDUCED_DEQUANTIZE(short coef, int quantval)
        {
            return ((int)coef * quantval);
        }
    }
}
