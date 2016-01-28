/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 * For conditions of distribution and use, see the accompanying README file.
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;

using BitMiracle.LibJpeg.Classic.Internal;

namespace BitMiracle.LibJpeg.Classic
{
    /// <summary>
    /// JPEG decompression routine.
    /// </summary>
    /// <seealso cref="jpeg_compress_struct"/>
#if EXPOSE_LIBJPEG
    public
#endif
    class jpeg_decompress_struct : jpeg_common_struct
    {
        /// <summary>
        /// The delegate for application-supplied marker processing methods.
        /// </summary>
        /// <param name="cinfo">Decompressor.</param>
        /// <returns>Return <c>true</c> to indicate success. <c>false</c> should be returned only 
        /// if you are using a suspending data source and it tells you to suspend.
        /// </returns>
        /// <remarks>Although the marker code is not explicitly passed, the routine can find it 
        /// in the <see cref="jpeg_decompress_struct.Unread_marker"/>. At the time of call, 
        /// the marker proper has been read from the data source module. The processor routine 
        /// is responsible for reading the marker length word and the remaining parameter bytes, if any.
        /// </remarks>
        public delegate bool jpeg_marker_parser_method(jpeg_decompress_struct cinfo);

        /* Source of compressed data */
        internal jpeg_source_mgr m_src;

        internal int m_image_width; /* nominal image width (from SOF marker) */
        internal int m_image_height;    /* nominal image height */
        internal int m_num_components;     /* # of color components in JPEG image */
        internal J_COLOR_SPACE m_jpeg_color_space; /* colorspace of JPEG image */

        internal J_COLOR_SPACE m_out_color_space; /* colorspace for output */
        internal int m_scale_num;
        internal int m_scale_denom; /* fraction by which to scale image */
        internal bool m_buffered_image;    /* true=multiple output passes */
        internal bool m_raw_data_out;      /* true=downsampled data wanted */
        internal J_DCT_METHOD m_dct_method;    /* IDCT algorithm selector */
        internal bool m_do_fancy_upsampling;   /* true=apply fancy upsampling */
        internal bool m_do_block_smoothing;    /* true=apply interblock smoothing */
        internal bool m_quantize_colors;   /* true=colormapped output wanted */
        internal J_DITHER_MODE m_dither_mode;  /* type of color dithering to use */
        internal bool m_two_pass_quantize; /* true=use two-pass color quantization */
        internal int m_desired_number_of_colors;   /* max # colors to use in created colormap */
        internal bool m_enable_1pass_quant;    /* enable future use of 1-pass quantizer */
        internal bool m_enable_external_quant;/* enable future use of external colormap */
        internal bool m_enable_2pass_quant;    /* enable future use of 2-pass quantizer */

        internal int m_output_width;    /* scaled image width */
        internal int m_output_height;   /* scaled image height */
        internal int m_out_color_components;   /* # of color components in out_color_space */
        /* # of color components returned
         * output_components is 1 (a colormap index) when quantizing colors;
         * otherwise it equals out_color_components.
         */
        internal int m_output_components;

        internal int m_rec_outbuf_height;  /* min recommended height of scanline buffer */

        internal int m_actual_number_of_colors;    /* number of entries in use */
        internal byte[][] m_colormap;     /* The color map as a 2-D pixel array */

        internal int m_output_scanline; /* 0 .. output_height-1  */

        internal int m_input_scan_number;  /* Number of SOS markers seen so far */
        internal int m_input_iMCU_row;  /* Number of iMCU rows completed */

        internal int m_output_scan_number; /* Nominal scan number being displayed */
        internal int m_output_iMCU_row; /* Number of iMCU rows read */

        internal int[][] m_coef_bits; /* -1 or current Al value for each coef */

        /* Internal JPEG parameters --- the application usually need not look at
         * these fields.  Note that the decompressor output side may not use
         * any parameters that can change between scans.
         */

        /* Quantization and Huffman tables are carried forward across input
         * datastreams when processing abbreviated JPEG datastreams.
         */

        internal JQUANT_TBL[] m_quant_tbl_ptrs = new JQUANT_TBL[JpegConstants.NUM_QUANT_TBLS];
        /* ptrs to coefficient quantization tables, or null if not defined */

        internal JHUFF_TBL[] m_dc_huff_tbl_ptrs = new JHUFF_TBL[JpegConstants.NUM_HUFF_TBLS];
        internal JHUFF_TBL[] m_ac_huff_tbl_ptrs = new JHUFF_TBL[JpegConstants.NUM_HUFF_TBLS];
        /* ptrs to Huffman coding tables, or null if not defined */

        /* These parameters are never carried across datastreams, since they
         * are given in SOF/SOS markers or defined to be reset by SOI.
         */

        internal int m_data_precision;     /* bits of precision in image data */

        /* m_comp_info[i] describes component that appears i'th in SOF */
        private jpeg_component_info[] m_comp_info;

        internal bool m_progressive_mode;  /* true if SOFn specifies progressive mode */

        internal int m_restart_interval; /* MCUs per restart interval, or 0 for no restart */

        /* These fields record data obtained from optional markers recognized by
         * the JPEG library.
         */
        internal bool m_saw_JFIF_marker;   /* true iff a JFIF APP0 marker was found */
        /* Data copied from JFIF marker; only valid if saw_JFIF_marker is true: */
        internal byte m_JFIF_major_version;   /* JFIF version number */
        internal byte m_JFIF_minor_version;

        internal DensityUnit m_density_unit;     /* JFIF code for pixel size units */
        internal short m_X_density;       /* Horizontal pixel density */
        internal short m_Y_density;       /* Vertical pixel density */

        internal bool m_saw_Adobe_marker;  /* true iff an Adobe APP14 marker was found */
        internal byte m_Adobe_transform;  /* Color transform code from Adobe marker */

        internal bool m_CCIR601_sampling;  /* true=first samples are cosited */

        internal List<jpeg_marker_struct> m_marker_list; /* Head of list of saved markers */
    
        /* Remaining fields are known throughout decompressor, but generally
         * should not be touched by a surrounding application.
         */

        /*
         * These fields are computed during decompression startup
         */
        internal int m_max_h_samp_factor;  /* largest h_samp_factor */
        internal int m_max_v_samp_factor;  /* largest v_samp_factor */
    
        internal int m_min_DCT_scaled_size;    /* smallest DCT_scaled_size of any component */

        internal int m_total_iMCU_rows; /* # of iMCU rows in image */
        /* The coefficient controller's input and output progress is measured in
         * units of "iMCU" (interleaved MCU) rows.  These are the same as MCU rows
         * in fully interleaved JPEG scans, but are used whether the scan is
         * interleaved or not.  We define an iMCU row as v_samp_factor DCT block
         * rows of each component.  Therefore, the IDCT output contains
         * v_samp_factor*DCT_scaled_size sample rows of a component per iMCU row.
         */

        internal byte[] m_sample_range_limit; /* table for fast range-limiting */
        internal int m_sampleRangeLimitOffset;

        /*
         * These fields are valid during any one scan.
         * They describe the components and MCUs actually appearing in the scan.
         * Note that the decompressor output side must not use these fields.
         */
        internal int m_comps_in_scan;      /* # of JPEG components in this scan */
        internal int[] m_cur_comp_info = new int[JpegConstants.MAX_COMPS_IN_SCAN];
        /* *cur_comp_info[i] describes component that appears i'th in SOS */

        internal int m_MCUs_per_row;    /* # of MCUs across the image */
        internal int m_MCU_rows_in_scan;    /* # of MCU rows in the image */

        internal int m_blocks_in_MCU;      /* # of DCT blocks per MCU */
        internal int[] m_MCU_membership = new int[JpegConstants.D_MAX_BLOCKS_IN_MCU];
        /* MCU_membership[i] is index in cur_comp_info of component owning */
        /* i'th block in an MCU */

        /* progressive JPEG parameters for scan */
        internal int m_Ss;
        internal int m_Se;
        internal int m_Ah;
        internal int m_Al;

        /* This field is shared between entropy decoder and marker parser.
         * It is either zero or the code of a JPEG marker that has been
         * read from the data source, but has not yet been processed.
         */
        internal int m_unread_marker;

        /*
         * Links to decompression subobjects (methods, private variables of modules)
         */
        internal jpeg_decomp_master m_master;
        internal jpeg_d_main_controller m_main;
        internal jpeg_d_coef_controller m_coef;
        internal jpeg_d_post_controller m_post;
        internal jpeg_input_controller m_inputctl;
        internal jpeg_marker_reader m_marker;
        internal jpeg_entropy_decoder m_entropy;
        internal jpeg_inverse_dct m_idct;
        internal jpeg_upsampler m_upsample;
        internal jpeg_color_deconverter m_cconvert;
        internal jpeg_color_quantizer m_cquantize;

        /// <summary>
        /// Initializes a new instance of the <see cref="jpeg_decompress_struct"/> class.
        /// </summary>
        /// <seealso cref="jpeg_compress_struct"/>
        public jpeg_decompress_struct()
        {
            initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="jpeg_decompress_struct"/> class.
        /// </summary>
        /// <param name="errorManager">The error manager.</param>
        /// <seealso cref="jpeg_compress_struct"/>
        public jpeg_decompress_struct(jpeg_error_mgr errorManager)
            : base(errorManager)
        {
            initialize();
        }

        /// <summary>
        /// Retrieves <c>true</c> because this is a decompressor.
        /// </summary>
        /// <value><c>true</c></value>
        public override bool IsDecompressor
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the source for decompression.
        /// </summary>
        /// <value>The source for decompression.</value>
        public LibJpeg.Classic.jpeg_source_mgr Src
        {
            get { return m_src; }
            set { m_src = value; }
        }

        /* Basic description of image --- filled in by jpeg_read_header(). */
        /* Application may inspect these values to decide how to process image. */

        /// <summary>
        /// Gets the width of image, set by <see cref="jpeg_decompress_struct.jpeg_read_header"/>
        /// </summary>
        /// <value>The width of image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Image_width
        {
            get { return m_image_width; }
        }

        /// <summary>
        /// Gets the height of image, set by <see cref="jpeg_decompress_struct.jpeg_read_header"/>
        /// </summary>
        /// <value>The height of image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Image_height
        {
            get { return m_image_height; }
        }
        
        /// <summary>
        /// Gets the number of color components in JPEG image.
        /// </summary>
        /// <value>The number of color components.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Num_components
        {
            get { return m_num_components; }
        }

        /// <summary>
        /// Gets or sets the colorspace of JPEG image.
        /// </summary>
        /// <value>The colorspace of JPEG image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.Classic.J_COLOR_SPACE Jpeg_color_space
        {
            get { return m_jpeg_color_space; }
            set { m_jpeg_color_space = value; }
        }

        /// <summary>
        /// Gets the list of loaded special markers.
        /// </summary>
        /// <remarks>All the special markers in the file appear in this list, in order of 
        /// their occurrence in the file (but omitting any markers of types you didn't ask for)
        /// </remarks>
        /// <value>The list of loaded special markers.</value>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public IReadOnlyList<jpeg_marker_struct> Marker_list
        {
            get
            {
                return m_marker_list;
            }
        }

        /* Decompression processing parameters --- these fields must be set before
         * calling jpeg_start_decompress().  Note that jpeg_read_header() initializes
         * them to default values.
         */

        /// <summary>
        /// Gets or sets the output color space.
        /// </summary>
        /// <value>The output color space.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.Classic.J_COLOR_SPACE Out_color_space
        {
            get { return m_out_color_space; }
            set { m_out_color_space = value; }
        }

        /// <summary>
        /// Gets or sets the numerator of the fraction of image scaling.
        /// </summary>
        /// <value>Scale the image by the fraction Scale_num/<see cref="jpeg_decompress_struct.Scale_denom">Scale_denom</see>. 
        /// Default is 1/1, or no scaling. Currently, the only supported scaling ratios are 1/1, 1/2, 1/4, and 1/8.
        /// (The library design allows for arbitrary scaling ratios but this is not likely to be implemented any time soon.)
        /// </value>
        /// <remarks>Smaller scaling ratios permit significantly faster decoding since fewer pixels 
        /// need to be processed and a simpler <see cref="J_DCT_METHOD">DCT method</see> can be used.</remarks>
        /// <seealso cref="jpeg_decompress_struct.Scale_denom"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Scale_num
        {
            get { return m_scale_num; }
            set { m_scale_num = value; }
        }

        /// <summary>
        /// Gets or sets the denominator of the fraction of image scaling.
        /// </summary>
        /// <value>Scale the image by the fraction <see cref="jpeg_decompress_struct.Scale_num">Scale_num</see>/Scale_denom. 
        /// Default is 1/1, or no scaling. Currently, the only supported scaling ratios are 1/1, 1/2, 1/4, and 1/8.
        /// (The library design allows for arbitrary scaling ratios but this is not likely to be implemented any time soon.)
        /// </value>
        /// <remarks>Smaller scaling ratios permit significantly faster decoding since fewer pixels 
        /// need to be processed and a simpler <see cref="J_DCT_METHOD">DCT method</see> can be used.</remarks>
        /// <seealso cref="jpeg_decompress_struct.Scale_num"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Scale_denom
        {
            get { return m_scale_denom; }
            set { m_scale_denom = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use buffered-image mode.
        /// </summary>
        /// <value><c>true</c> if buffered-image mode is turned on; otherwise, <c>false</c>.</value>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Buffered_image
        {
            get { return m_buffered_image; }
            set { m_buffered_image = value; }
        }

        /// <summary>
        /// Enable or disable raw data output.
        /// </summary>
        /// <value><c>true</c> if raw data output is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>Default value: <c>false</c><br/>
        /// Set this to true before <see cref="jpeg_decompress_struct.jpeg_start_decompress"/> 
        /// if you need to obtain raw data output.
        /// </remarks>
        /// <seealso cref="jpeg_read_raw_data"/>
        public bool Raw_data_out
        {
            get { return m_raw_data_out; }
            set { m_raw_data_out = value; }
        }

        /// <summary>
        /// Gets or sets the algorithm used for the DCT step.
        /// </summary>
        /// <value>The algorithm used for the DCT step.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.Classic.J_DCT_METHOD Dct_method
        {
            get { return m_dct_method; }
            set { m_dct_method = value; }
        }
        
        /// <summary>
        /// Enable or disable upsampling of chroma components.
        /// </summary>
        /// <value>If <c>true</c>, do careful upsampling of chroma components. 
        /// If <c>false</c>, a faster but sloppier method is used. 
        /// The visual impact of the sloppier method is often very small.
        /// </value>
        /// <remarks>Default value: <c>true</c></remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Do_fancy_upsampling
        {
            get { return m_do_fancy_upsampling; }
            set { m_do_fancy_upsampling = value; }
        }
        
        /// <summary>
        /// Apply interblock smoothing in early stages of decoding progressive JPEG files.
        /// </summary>
        /// <value>If <c>true</c>, interblock smoothing is applied in early stages of decoding progressive JPEG files; 
        /// if <c>false</c>, not. Early progression stages look "fuzzy" with smoothing, "blocky" without.</value>
        /// <remarks>Default value: <c>true</c><br/>
        /// In any case, block smoothing ceases to be applied after the first few AC coefficients are 
        /// known to full accuracy, so it is relevant only when using 
        /// <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> for progressive images.
        /// </remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Do_block_smoothing
        {
            get { return m_do_block_smoothing; }
            set { m_do_block_smoothing = value; }
        }
        
        /// <summary>
        /// Colors quantization.
        /// </summary>
        /// <value>If set <c>true</c>, colormapped output will be delivered.<br/>
        /// Default value: <c>false</c>, meaning that full-color output will be delivered.
        /// </value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Quantize_colors
        {
            get { return m_quantize_colors; }
            set { m_quantize_colors = value; }
        }

        /* the following are ignored if not quantize_colors: */
        
        /// <summary>
        /// Selects color dithering method.
        /// </summary>
        /// <value>Default value: <see cref="J_DITHER_MODE.JDITHER_FS"/>.</value>
        /// <remarks>Ignored if <see cref="jpeg_decompress_struct.Quantize_colors"/> is <c>false</c>.<br/>
        /// At present, ordered dither is implemented only in the single-pass, standard-colormap case. 
        /// If you ask for ordered dither when <see cref="jpeg_decompress_struct.Two_pass_quantize"/> is <c>true</c>
        /// or when you supply an external color map, you'll get F-S dithering.
        /// </remarks>
        /// <seealso cref="jpeg_decompress_struct.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.Classic.J_DITHER_MODE Dither_mode
        {
            get { return m_dither_mode; }
            set { m_dither_mode = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to use two-pass color quantization.
        /// </summary>
        /// <value>If <c>true</c>, an extra pass over the image is made to select a custom color map for the image.
        /// This usually looks a lot better than the one-size-fits-all colormap that is used otherwise.
        /// Ignored when the application supplies its own color map.<br/>
        /// 
        /// Default value: <c>true</c>
        /// </value>
        /// <remarks>Ignored if <see cref="jpeg_decompress_struct.Quantize_colors"/> is <c>false</c>.<br/>
        /// </remarks>
        /// <seealso cref="jpeg_decompress_struct.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Two_pass_quantize
        {
            get { return m_two_pass_quantize; }
            set { m_two_pass_quantize = value; }
        }

        /// <summary>
        /// Maximum number of colors to use in generating a library-supplied color map.
        /// </summary>
        /// <value>Default value: 256.</value>
        /// <remarks>Ignored if <see cref="jpeg_decompress_struct.Quantize_colors"/> is <c>false</c>.<br/>
        /// The actual number of colors is returned in a <see cref="jpeg_decompress_struct.Actual_number_of_colors"/>.
        /// </remarks>
        /// <seealso cref="jpeg_decompress_struct.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Desired_number_of_colors
        {
            get { return m_desired_number_of_colors; }
            set { m_desired_number_of_colors = value; }
        }
        
        /* these are significant only in buffered-image mode: */
        
        /// <summary>
        /// Enable future use of 1-pass quantizer.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_1pass_quant
        {
            get { return m_enable_1pass_quant; }
            set { m_enable_1pass_quant = value; }
        }
        
        /// <summary>
        /// Enable future use of external colormap.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_external_quant
        {
            get { return m_enable_external_quant; }
            set { m_enable_external_quant = value; }
        }

        /// <summary>
        /// Enable future use of 2-pass quantizer.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_2pass_quant
        {
            get { return m_enable_2pass_quant; }
            set { m_enable_2pass_quant = value; }
        }

        /* Description of actual output image that will be returned to application.
         * These fields are computed by jpeg_start_decompress().
         * You can also use jpeg_calc_output_dimensions() to determine these values
         * in advance of calling jpeg_start_decompress().
         */

        /// <summary>
        /// Gets the actual width of output image.
        /// </summary>
        /// <value>The width of output image.</value>
        /// <remarks>Computed by <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.
        /// You can also use <see cref="jpeg_decompress_struct.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.</remarks>
        /// <seealso cref="jpeg_decompress_struct.Output_height"/>
        public int Output_width
        {
            get { return m_output_width; }
        }

        /// <summary>
        /// Gets the actual height of output image.
        /// </summary>
        /// <value>The height of output image.</value>
        /// <remarks>Computed by <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.
        /// You can also use <see cref="jpeg_decompress_struct.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.</remarks>
        /// <seealso cref="jpeg_decompress_struct.Output_width"/>
        public int Output_height
        {
            get { return m_output_height; }
        }
        
        /// <summary>
        /// Gets the number of color components in <see cref="jpeg_decompress_struct.Out_color_space"/>.
        /// </summary>
        /// <remarks>Computed by <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.
        /// You can also use <see cref="jpeg_decompress_struct.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.</remarks>
        /// <value>The number of color components.</value>
        /// <seealso cref="jpeg_decompress_struct.Out_color_space"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Out_color_components
        {
            get { return m_out_color_components; }
        }

        /// <summary>
        /// Gets the number of color components returned.
        /// </summary>
        /// <remarks>Computed by <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.
        /// You can also use <see cref="jpeg_decompress_struct.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.</remarks>
        /// <value>When <see cref="jpeg_decompress_struct.Quantize_colors">quantizing colors</see>, 
        /// <c>Output_components</c> is 1, indicating a single color map index per pixel. 
        /// Otherwise it equals to <see cref="jpeg_decompress_struct.Out_color_components"/>.
        /// </value>
        /// <seealso cref="jpeg_decompress_struct.Out_color_space"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Output_components
        {
            get { return m_output_components; }
        }

        /// <summary>
        /// Gets the recommended height of scanline buffer.
        /// </summary>
        /// <value>In high-quality modes, <c>Rec_outbuf_height</c> is always 1, but some faster, 
        /// lower-quality modes set it to larger values (typically 2 to 4).</value>
        /// <remarks>Computed by <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.
        /// You can also use <see cref="jpeg_decompress_struct.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>.<br/>
        /// 
        /// <c>Rec_outbuf_height</c> is the recommended minimum height (in scanlines) 
        /// of the buffer passed to <see cref="jpeg_decompress_struct.jpeg_read_scanlines"/>.
        /// If the buffer is smaller, the library will still work, but time will be wasted due 
        /// to unnecessary data copying. If you are going to ask for a high-speed processing mode, 
        /// you may as well go to the trouble of honoring <c>Rec_outbuf_height</c> so as to avoid data copying.
        /// (An output buffer larger than <c>Rec_outbuf_height</c> lines is OK, but won't provide 
        /// any material speed improvement over that height.)
        /// </remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Rec_outbuf_height
        {
            get { return m_rec_outbuf_height; }
        }

        /* When quantizing colors, the output colormap is described by these fields.
         * The application can supply a colormap by setting colormap non-null before
         * calling jpeg_start_decompress; otherwise a colormap is created during
         * jpeg_start_decompress or jpeg_start_output.
         * The map has out_color_components rows and actual_number_of_colors columns.
         */
        
        /// <summary>
        /// The number of colors in the color map.
        /// </summary>
        /// <value>The number of colors in the color map.</value>
        /// <seealso cref="jpeg_decompress_struct.Colormap"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Actual_number_of_colors
        {
            get { return m_actual_number_of_colors; }
            set { m_actual_number_of_colors = value; }
        }

        /// <summary>
        /// The color map, represented as a 2-D pixel array of <see cref="jpeg_decompress_struct.Out_color_components"/> rows 
        /// and <see cref="jpeg_decompress_struct.Actual_number_of_colors"/> columns.
        /// </summary>
        /// <value>Colormap is set to <c>null</c> by <see cref="jpeg_decompress_struct.jpeg_read_header"/>.
        /// The application can supply a color map by setting <c>Colormap</c> non-null and setting 
        /// <see cref="jpeg_decompress_struct.Actual_number_of_colors"/> to the map size.
        /// </value>
        /// <remarks>Ignored if not quantizing.<br/>
        /// Implementation restriction: at present, an externally supplied <c>Colormap</c>
        /// is only accepted for 3-component output color spaces.
        /// </remarks>
        /// <seealso cref="jpeg_decompress_struct.Actual_number_of_colors"/>
        /// <seealso cref="jpeg_decompress_struct.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public byte[][] Colormap
        {
            get { return m_colormap; }
            set { m_colormap = value; }
        }

        /* State variables: these variables indicate the progress of decompression.
         * The application may examine these but must not modify them.
         */

        /* Row index of next scanline to be read from jpeg_read_scanlines().
         * Application may use this to control its processing loop, e.g.,
         * "while (output_scanline < output_height)".
         */
        
        /// <summary>
        /// Gets the number of scanlines returned so far.
        /// </summary>
        /// <value>The output_scanline.</value>
        /// <remarks>Usually you can just use this variable as the loop counter, 
        /// so that the loop test looks like 
        /// <c>while (cinfo.Output_scanline &lt; cinfo.Output_height)</c></remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public int Output_scanline
        {
            get { return m_output_scanline; }
        }

        /* Current input scan number and number of iMCU rows completed in scan.
         * These indicate the progress of the decompressor input side.
         */

        /// <summary>
        /// Gets the number of SOS markers seen so far.
        /// </summary>
        /// <value>The number of SOS markers seen so far.</value>
        /// <remarks>Indicates the progress of the decompressor input side.</remarks>
        public int Input_scan_number
        {
            get { return m_input_scan_number; }
        }

        /// <summary>
        /// Gets the number of iMCU rows completed.
        /// </summary>
        /// <value>The number of iMCU rows completed.</value>
        /// <remarks>Indicates the progress of the decompressor input side.</remarks>
        public int Input_iMCU_row
        {
            get { return m_input_iMCU_row; }
        }

        /* The "output scan number" is the notional scan being displayed by the
         * output side.  The decompressor will not allow output scan/row number
         * to get ahead of input scan/row, but it can fall arbitrarily far behind.
         */
        
        /// <summary>
        /// Gets the nominal scan number being displayed.
        /// </summary>
        /// <value>The nominal scan number being displayed.</value>
        public int Output_scan_number
        {
            get { return m_output_scan_number; }
        }
        
        /// <summary>
        /// Gets the number of iMCU rows read.
        /// </summary>
        /// <value>The number of iMCU rows read.</value>
        public int Output_iMCU_row
        {
            get { return m_output_iMCU_row; }
        }

        /* Current progression status.  coef_bits[c][i] indicates the precision
         * with which component c's DCT coefficient i (in zigzag order) is known.
         * It is -1 when no data has yet been received, otherwise it is the point
         * transform (shift) value for the most recent scan of the coefficient
         * (thus, 0 at completion of the progression).
         * This is null when reading a non-progressive file.
         */
        
        /// <summary>
        /// Gets the current progression status..
        /// </summary>
        /// <value><c>Coef_bits[c][i]</c> indicates the precision with 
        /// which component c's DCT coefficient i (in zigzag order) is known. 
        /// It is <c>-1</c> when no data has yet been received, otherwise 
        /// it is the point transform (shift) value for the most recent scan of the coefficient 
        /// (thus, 0 at completion of the progression). This is null when reading a non-progressive file.
        /// </value>
        /// <seealso href="bda5b19b-88e0-44bf-97de-cd30fc61bb65.htm" target="_self">Progressive JPEG support</seealso>
        public int[][] Coef_bits
        {
            get { return m_coef_bits; }
        }

        // These fields record data obtained from optional markers 
        // recognized by the JPEG library.

        /// <summary>
        /// Gets the resolution information from JFIF marker.
        /// </summary>
        /// <value>The information from JFIF marker.</value>
        /// <seealso cref="jpeg_decompress_struct.X_density"/>
        /// <seealso cref="jpeg_decompress_struct.Y_density"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public DensityUnit Density_unit
        {
            get { return m_density_unit; }
        }

        /// <summary>
        /// Gets the horizontal component of pixel ratio.
        /// </summary>
        /// <value>The horizontal component of pixel ratio.</value>
        /// <seealso cref="jpeg_decompress_struct.Y_density"/>
        /// <seealso cref="jpeg_decompress_struct.Density_unit"/>
        public short X_density
        {
            get { return m_X_density; }
        }

        /// <summary>
        /// Gets the vertical component of pixel ratio.
        /// </summary>
        /// <value>The vertical component of pixel ratio.</value>
        /// <seealso cref="jpeg_decompress_struct.X_density"/>
        /// <seealso cref="jpeg_decompress_struct.Density_unit"/>
        public short Y_density
        {
            get { return m_Y_density; }
        }

        /// <summary>
        /// Gets the data precision.
        /// </summary>
        /// <value>The data precision.</value>
        public int Data_precision
        {
            get { return m_data_precision; }
            //set { m_data_precision = value; }
        }

        /// <summary>
        /// Gets the largest vertical sample factor.
        /// </summary>
        /// <value>The largest vertical sample factor.</value>
        public int Max_v_samp_factor
        {
            get { return m_max_v_samp_factor; }
            //set { m_max_v_samp_factor = value; }
        }

        /// <summary>
        /// Gets the last read and unprocessed JPEG marker.
        /// </summary>
        /// <value>It is either zero or the code of a JPEG marker that has been
        /// read from the data source, but has not yet been processed.
        /// </value>
        /// <seealso cref="jpeg_decompress_struct.jpeg_set_marker_processor"/>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public int Unread_marker
        {
            get { return m_unread_marker; }
        }

        /// <summary>
        /// Comp_info[i] describes component that appears i'th in SOF
        /// </summary>
        /// <value>The components in SOF.</value>
        /// <seealso cref="jpeg_component_info"/>
        public jpeg_component_info[] Comp_info
        {
            get { return m_comp_info; }
            internal set { m_comp_info = value; }
        }

        /// <summary>
        /// Sets input stream.
        /// </summary>
        /// <param name="infile">The input stream.</param>
        /// <remarks>
        /// The caller must have already opened the stream, and is responsible
        /// for closing it after finishing decompression.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public void jpeg_stdio_src(Stream infile)
        {
            /* The source object and input buffer are made permanent so that a series
            * of JPEG images can be read from the same file by calling jpeg_stdio_src
            * only before the first one.  (If we discarded the buffer at the end of
            * one image, we'd likely lose the start of the next one.)
            * This makes it unsafe to use this manager and a different source
            * manager serially with the same JPEG object.  Caveat programmer.
            */
            if (m_src == null)
            {
                /* first time for this JPEG object? */
                m_src = new my_source_mgr(this);
            }

            my_source_mgr m = m_src as my_source_mgr;
            if (m != null)
                m.Attach(infile);
        }

        /// <summary>
        /// Decompression startup: this will read the source datastream header markers, up to the beginning of the compressed data proper.
        /// </summary>
        /// <param name="require_image">Read a description of <b>Return Value</b>.</param>
        /// <returns>
        /// If you pass <c>require_image=true</c> (normal case), you need not check for a
        /// <see cref="ReadResult.JPEG_HEADER_TABLES_ONLY"/> return code; an abbreviated file will cause
        /// an error exit. <see cref="ReadResult.JPEG_SUSPENDED"/> is only possible if you use a data source
        /// module that can give a suspension return.<br/><br/>
        /// 
        /// This method will read as far as the first SOS marker (ie, actual start of compressed data),
        /// and will save all tables and parameters in the JPEG object. It will also initialize the
        /// decompression parameters to default values, and finally return <see cref="ReadResult.JPEG_HEADER_OK"/>.
        /// On return, the application may adjust the decompression parameters and then call
        /// <see cref="jpeg_decompress_struct.jpeg_start_decompress"/>. (Or, if the application only wanted to
        /// determine the image parameters, the data need not be decompressed. In that case, call
        /// <see cref="jpeg_common_struct.jpeg_abort"/> to release any temporary space.)<br/><br/>
        /// 
        /// If an abbreviated (tables only) datastream is presented, the routine will return
        /// <see cref="ReadResult.JPEG_HEADER_TABLES_ONLY"/> upon reaching EOI. The application may then re-use
        /// the JPEG object to read the abbreviated image datastream(s). It is unnecessary (but OK) to call
        /// <see cref="jpeg_common_struct.jpeg_abort">jpeg_abort</see> in this case.
        /// The <see cref="ReadResult.JPEG_SUSPENDED"/> return code only occurs if the data source module
        /// requests suspension of the decompressor. In this case the application should load more source
        /// data and then re-call <c>jpeg_read_header</c> to resume processing.<br/><br/>
        /// 
        /// If a non-suspending data source is used and <c>require_image</c> is <c>true</c>,
        /// then the return code need not be inspected since only <see cref="ReadResult.JPEG_HEADER_OK"/> is possible.
        /// </returns>
        /// <remarks>Need only initialize JPEG object and supply a data source before calling.<br/>
        /// On return, the image dimensions and other info have been stored in the JPEG object.
        /// The application may wish to consult this information before selecting decompression parameters.<br/>
        /// This routine is now just a front end to <see cref="jpeg_consume_input"/>, with some extra error checking.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public ReadResult jpeg_read_header(bool require_image)
        {
            if (m_global_state != JpegState.DSTATE_START && m_global_state != JpegState.DSTATE_INHEADER)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            ReadResult retcode = jpeg_consume_input();

            switch (retcode)
            {
                case ReadResult.JPEG_REACHED_SOS:
                    return ReadResult.JPEG_HEADER_OK;
                case ReadResult.JPEG_REACHED_EOI:
                    if (require_image)      /* Complain if application wanted an image */
                        ERREXIT(J_MESSAGE_CODE.JERR_NO_IMAGE);
                    /* Reset to start state; it would be safer to require the application to
                    * call jpeg_abort, but we can't change it now for compatibility reasons.
                    * A side effect is to free any temporary memory (there shouldn't be any).
                    */
                    jpeg_abort(); /* sets state = DSTATE_START */
                    return ReadResult.JPEG_HEADER_TABLES_ONLY;

                case ReadResult.JPEG_SUSPENDED:
                    /* no work */
                    break;
            }

            return ReadResult.JPEG_SUSPENDED;
        }

        //////////////////////////////////////////////////////////////////////////
        // Main entry points for decompression

        /// <summary>
        /// Decompression initialization.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected 
        /// only if a suspending data source is used.
        /// </returns>
        /// <remarks><see cref="jpeg_decompress_struct.jpeg_read_header">jpeg_read_header</see> must be completed before calling this.<br/>
        /// 
        /// If a multipass operating mode was selected, this will do all but the last pass, and thus may take a great deal of time.
        /// </remarks>
        /// <seealso cref="jpeg_decompress_struct.jpeg_finish_decompress"/>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public bool jpeg_start_decompress()
        {
            if (m_global_state == JpegState.DSTATE_READY)
            {
                /* First call: initialize master control, select active modules */
                m_master = new jpeg_decomp_master(this);
                if (m_buffered_image)
                {
                    /* No more work here; expecting jpeg_start_output next */
                    m_global_state = JpegState.DSTATE_BUFIMAGE;
                    return true;
                }
                m_global_state = JpegState.DSTATE_PRELOAD;
            }

            if (m_global_state == JpegState.DSTATE_PRELOAD)
            {
                /* If file has multiple scans, absorb them all into the coef buffer */
                if (m_inputctl.HasMultipleScans())
                {
                    for ( ; ; )
                    {
                        ReadResult retcode;
                        /* Call progress monitor hook if present */
                        if (m_progress != null)
                            m_progress.Updated();

                        /* Absorb some more input */
                        retcode = m_inputctl.consume_input();
                        if (retcode == ReadResult.JPEG_SUSPENDED)
                            return false;

                        if (retcode == ReadResult.JPEG_REACHED_EOI)
                            break;

                        /* Advance progress counter if appropriate */
                        if (m_progress != null && (retcode == ReadResult.JPEG_ROW_COMPLETED || retcode == ReadResult.JPEG_REACHED_SOS))
                        {
                            m_progress.Pass_counter++;
                            if (m_progress.Pass_counter >= m_progress.Pass_limit)
                            {
                                /* underestimated number of scans; ratchet up one scan */
                                m_progress.Pass_limit += m_total_iMCU_rows;
                            }
                        }
                    }
                }

                m_output_scan_number = m_input_scan_number;
            }
            else if (m_global_state != JpegState.DSTATE_PRESCAN)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            /* Perform any dummy output passes, and set up for the final pass */
            return output_pass_setup();
        }

        /// <summary>
        /// Read some scanlines of data from the JPEG decompressor.
        /// </summary>
        /// <param name="scanlines">Buffer for filling.</param>
        /// <param name="max_lines">Required number of lines.</param>
        /// <returns>The return value will be the number of lines actually read. 
        /// This may be less than the number requested in several cases, including 
        /// bottom of image, data source suspension, and operating modes that emit multiple scanlines at a time.
        /// </returns>
        /// <remarks>We warn about excess calls to <c>jpeg_read_scanlines</c> since this likely signals an 
        /// application programmer error. However, an oversize buffer <c>(max_lines > scanlines remaining)</c> 
        /// is not an error.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public int jpeg_read_scanlines(byte[][] scanlines, int max_lines)
        {
            if (m_global_state != JpegState.DSTATE_SCANNING)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            if (m_output_scanline >= m_output_height)
            {
                WARNMS(J_MESSAGE_CODE.JWRN_TOO_MUCH_DATA);
                return 0;
            }

            /* Call progress monitor hook if present */
            if (m_progress != null)
            {
                m_progress.Pass_counter = m_output_scanline;
                m_progress.Pass_limit = m_output_height;
                m_progress.Updated();
            }

            /* Process some data */
            int row_ctr = 0;
            m_main.process_data(scanlines, ref row_ctr, max_lines);
            m_output_scanline += row_ctr;
            return row_ctr;
        }

        /// <summary>
        /// Finish JPEG decompression.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected 
        /// only if a suspending data source is used.
        /// </returns>
        /// <remarks>This will normally just verify the file trailer and release temp storage.</remarks>
        /// <seealso cref="jpeg_decompress_struct.jpeg_start_decompress"/>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public bool jpeg_finish_decompress()
        {
            if ((m_global_state == JpegState.DSTATE_SCANNING || m_global_state == JpegState.DSTATE_RAW_OK) && !m_buffered_image)
            {
                /* Terminate final pass of non-buffered mode */
                if (m_output_scanline < m_output_height)
                    ERREXIT(J_MESSAGE_CODE.JERR_TOO_LITTLE_DATA);

                m_master.finish_output_pass();
                m_global_state = JpegState.DSTATE_STOPPING;
            }
            else if (m_global_state == JpegState.DSTATE_BUFIMAGE)
            {
                /* Finishing after a buffered-image operation */
                m_global_state = JpegState.DSTATE_STOPPING;
            }
            else if (m_global_state != JpegState.DSTATE_STOPPING)
            {
                /* STOPPING = repeat call after a suspension, anything else is error */
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);
            }

            /* Read until EOI */
            while (!m_inputctl.EOIReached())
            {
                if (m_inputctl.consume_input() == ReadResult.JPEG_SUSPENDED)
                {
                    /* Suspend, come back later */
                    return false;
                }
            }

            /* Do final cleanup */
            m_src.term_source();

            /* We can use jpeg_abort to release memory and reset global_state */
            jpeg_abort();
            return true;
        }

        /// <summary>
        /// Alternate entry point to read raw data.
        /// </summary>
        /// <param name="data">The raw data.</param>
        /// <param name="max_lines">The number of scanlines for reading.</param>
        /// <returns>The number of lines actually read.</returns>
        /// <remarks>Replaces <see cref="jpeg_decompress_struct.jpeg_read_scanlines">jpeg_read_scanlines</see> 
        /// when reading raw downsampled data. Processes exactly one iMCU row per call, unless suspended.
        /// </remarks>
        public int jpeg_read_raw_data(byte[][][] data, int max_lines)
        {
            if (m_global_state != JpegState.DSTATE_RAW_OK)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            if (m_output_scanline >= m_output_height)
            {
                WARNMS(J_MESSAGE_CODE.JWRN_TOO_MUCH_DATA);
                return 0;
            }

            /* Call progress monitor hook if present */
            if (m_progress != null)
            {
                m_progress.Pass_counter = m_output_scanline;
                m_progress.Pass_limit = m_output_height;
                m_progress.Updated();
            }

            /* Verify that at least one iMCU row can be returned. */
            int lines_per_iMCU_row = m_max_v_samp_factor * m_min_DCT_scaled_size;
            if (max_lines < lines_per_iMCU_row)
                ERREXIT(J_MESSAGE_CODE.JERR_BUFFER_SIZE);

            int componentCount = data.Length; // maybe we should use max_lines here
            ComponentBuffer[] cb = new ComponentBuffer[componentCount];
            for (int i = 0; i < componentCount; i++)
            {
                cb[i] = new ComponentBuffer();
                cb[i].SetBuffer(data[i], null, 0);
            }

            /* Decompress directly into user's buffer. */
            if (m_coef.decompress_data(cb) == ReadResult.JPEG_SUSPENDED)
            {
                /* suspension forced, can do nothing more */
                return 0;
            }

            /* OK, we processed one iMCU row. */
            m_output_scanline += lines_per_iMCU_row;
            return lines_per_iMCU_row;
        }

        //////////////////////////////////////////////////////////////////////////
        // Additional entry points for buffered-image mode.

        /// <summary>
        /// Is there more than one scan?
        /// </summary>
        /// <returns><c>true</c> if image has more than one scan; otherwise, <c>false</c></returns>
        /// <remarks>If you are concerned about maximum performance on baseline JPEG files,
        /// you should use <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> only
        /// when the incoming file actually has multiple scans. This can be tested by calling this method.
        /// </remarks>
        public bool jpeg_has_multiple_scans()
        {
            /* Only valid after jpeg_read_header completes */
            if (m_global_state < JpegState.DSTATE_READY || m_global_state > JpegState.DSTATE_STOPPING)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            return m_inputctl.HasMultipleScans();
        }

        /// <summary>
        /// Initialize for an output pass in <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see>.
        /// </summary>
        /// <param name="scan_number">Indicates which scan of the input file is to be displayed; 
        /// the scans are numbered starting at 1 for this purpose.</param>
        /// <returns><c>true</c> if done; <c>false</c> if suspended</returns>
        /// <seealso cref="jpeg_decompress_struct.jpeg_finish_output"/>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_start_output(int scan_number)
        {
            if (m_global_state != JpegState.DSTATE_BUFIMAGE && m_global_state != JpegState.DSTATE_PRESCAN)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            /* Limit scan number to valid range */
            if (scan_number <= 0)
                scan_number = 1;

            if (m_inputctl.EOIReached() && scan_number > m_input_scan_number)
                scan_number = m_input_scan_number;

            m_output_scan_number = scan_number;
            /* Perform any dummy output passes, and set up for the real pass */
            return output_pass_setup();
        }

        /// <summary>
        /// Finish up after an output pass in <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see>.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected only if a suspending data source is used.</returns>
        /// <seealso cref="jpeg_decompress_struct.jpeg_start_output"/>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_finish_output()
        {
            if ((m_global_state == JpegState.DSTATE_SCANNING || m_global_state == JpegState.DSTATE_RAW_OK) && m_buffered_image)
            {
                /* Terminate this pass. */
                /* We do not require the whole pass to have been completed. */
                m_master.finish_output_pass();
                m_global_state = JpegState.DSTATE_BUFPOST;
            }
            else if (m_global_state != JpegState.DSTATE_BUFPOST)
            {
                /* BUFPOST = repeat call after a suspension, anything else is error */
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);
            }

            /* Read markers looking for SOS or EOI */
            while (m_input_scan_number <= m_output_scan_number && !m_inputctl.EOIReached())
            {
                if (m_inputctl.consume_input() == ReadResult.JPEG_SUSPENDED)
                {
                    /* Suspend, come back later */
                    return false;
                }
            }

            m_global_state = JpegState.DSTATE_BUFIMAGE;
            return true;
        }

        /// <summary>
        /// Indicates if we have finished reading the input file.
        /// </summary>
        /// <returns><c>true</c> if we have finished reading the input file.</returns>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_input_complete()
        {
            /* Check for valid jpeg object */
            if (m_global_state < JpegState.DSTATE_START || m_global_state > JpegState.DSTATE_STOPPING)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            return m_inputctl.EOIReached();
        }

        /// <summary>
        /// Consume data in advance of what the decompressor requires.
        /// </summary>
        /// <returns>The result of data consumption.</returns>
        /// <remarks>This routine can be called at any time after initializing the JPEG object.
        /// It reads some additional data and returns when one of the indicated significant events
        /// occurs. If called after the EOI marker is reached, it will immediately return
        /// <see cref="ReadResult.JPEG_REACHED_EOI"/> without attempting to read more data.</remarks>
        public ReadResult jpeg_consume_input()
        {
            ReadResult retcode = ReadResult.JPEG_SUSPENDED;

            /* NB: every possible DSTATE value should be listed in this switch */
            switch (m_global_state)
            {
                case JpegState.DSTATE_START:
                    jpeg_consume_input_start();
                    retcode = jpeg_consume_input_inHeader();
                    break;
                case JpegState.DSTATE_INHEADER:
                    retcode = jpeg_consume_input_inHeader();
                    break;
                case JpegState.DSTATE_READY:
                    /* Can't advance past first SOS until start_decompress is called */
                    retcode = ReadResult.JPEG_REACHED_SOS;
                    break;
                case JpegState.DSTATE_PRELOAD:
                case JpegState.DSTATE_PRESCAN:
                case JpegState.DSTATE_SCANNING:
                case JpegState.DSTATE_RAW_OK:
                case JpegState.DSTATE_BUFIMAGE:
                case JpegState.DSTATE_BUFPOST:
                case JpegState.DSTATE_STOPPING:
                    retcode = m_inputctl.consume_input();
                    break;
                default:
                    ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);
                    break;
            }
            return retcode;
        }

        /// <summary>
        /// Pre-calculate output image dimensions and related values for current decompression parameters.
        /// </summary>
        /// <remarks>This is allowed for possible use by application. Hence it mustn't do anything 
        /// that can't be done twice. Also note that it may be called before the master module is initialized!
        /// </remarks>
        public void jpeg_calc_output_dimensions()
        {
            // Do computations that are needed before master selection phase
            /* Prevent application from calling me at wrong times */
            if (m_global_state != JpegState.DSTATE_READY)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);

            /* Compute actual output image dimensions and DCT scaling choices. */
            if (m_scale_num * 8 <= m_scale_denom)
            {
                /* Provide 1/8 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 8);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 8);
                m_min_DCT_scaled_size = 1;
            }
            else if (m_scale_num * 4 <= m_scale_denom)
            {
                /* Provide 1/4 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 4);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 4);
                m_min_DCT_scaled_size = 2;
            }
            else if (m_scale_num * 2 <= m_scale_denom)
            {
                /* Provide 1/2 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 2);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 2);
                m_min_DCT_scaled_size = 4;
            }
            else
            {
                /* Provide 1/1 scaling */
                m_output_width = m_image_width;
                m_output_height = m_image_height;
                m_min_DCT_scaled_size = JpegConstants.DCTSIZE;
            }

            /* In selecting the actual DCT scaling for each component, we try to
            * scale up the chroma components via IDCT scaling rather than upsampling.
            * This saves time if the upsampler gets to use 1:1 scaling.
            * Note this code assumes that the supported DCT scalings are powers of 2.
            */
            for (int ci = 0; ci < m_num_components; ci++)
            {
                int ssize = m_min_DCT_scaled_size;
                while (ssize < JpegConstants.DCTSIZE && 
                    (m_comp_info[ci].H_samp_factor * ssize * 2 <= m_max_h_samp_factor * m_min_DCT_scaled_size) &&
                    (m_comp_info[ci].V_samp_factor * ssize * 2 <= m_max_v_samp_factor * m_min_DCT_scaled_size))
                {
                    ssize = ssize * 2;
                }

                m_comp_info[ci].DCT_scaled_size = ssize;
            }

            /* Recompute downsampled dimensions of components;
            * application needs to know these if using raw downsampled data.
            */
            for (int ci = 0; ci < m_num_components; ci++)
            {
                /* Size in samples, after IDCT scaling */
                m_comp_info[ci].downsampled_width = JpegUtils.jdiv_round_up(
                    m_image_width * m_comp_info[ci].H_samp_factor * m_comp_info[ci].DCT_scaled_size,
                    m_max_h_samp_factor * JpegConstants.DCTSIZE);

                m_comp_info[ci].downsampled_height = JpegUtils.jdiv_round_up(
                    m_image_height * m_comp_info[ci].V_samp_factor * m_comp_info[ci].DCT_scaled_size,
                    m_max_v_samp_factor * JpegConstants.DCTSIZE);
            }

            /* Report number of components in selected colorspace. */
            /* Probably this should be in the color conversion module... */
            switch (m_out_color_space)
            {
                case J_COLOR_SPACE.JCS_GRAYSCALE:
                    m_out_color_components = 1;
                    break;
                case J_COLOR_SPACE.JCS_RGB:
                case J_COLOR_SPACE.JCS_YCbCr:
                    m_out_color_components = 3;
                    break;
                case J_COLOR_SPACE.JCS_CMYK:
                case J_COLOR_SPACE.JCS_YCCK:
                    m_out_color_components = 4;
                    break;
                default:
                    /* else must be same colorspace as in file */
                    m_out_color_components = m_num_components;
                    break;
            }

            m_output_components = (m_quantize_colors ? 1 : m_out_color_components);

            /* See if upsampler will want to emit more than one row at a time */
            if (use_merged_upsample())
                m_rec_outbuf_height = m_max_v_samp_factor;
            else
                m_rec_outbuf_height = 1;
        }

        /// <summary>
        /// Read or write the raw DCT coefficient arrays from a JPEG file (useful for lossless transcoding).
        /// </summary>
        /// <returns>Returns <c>null</c> if suspended. This case need be checked only 
        /// if a suspending data source is used.
        /// </returns>
        /// <remarks>
        /// <see cref="jpeg_decompress_struct.jpeg_read_header">jpeg_read_header</see> must be completed before calling this.<br/>
        /// 
        /// The entire image is read into a set of virtual coefficient-block arrays, one per component.
        /// The return value is an array of virtual-array descriptors.<br/>
        /// 
        /// An alternative usage is to simply obtain access to the coefficient arrays during a 
        /// <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> decompression operation. This is allowed after any 
        /// <see cref="jpeg_decompress_struct.jpeg_finish_output">jpeg_finish_output</see> call. The arrays can be accessed 
        /// until <see cref="jpeg_decompress_struct.jpeg_finish_decompress">jpeg_finish_decompress</see> is called. 
        /// Note that any call to the library may reposition the arrays, 
        /// so don't rely on <see cref="jvirt_array{T}.Access"/> results to stay valid across library calls.
        /// </remarks>
        public jvirt_array<JBLOCK>[] jpeg_read_coefficients()
        {
            if (m_global_state == JpegState.DSTATE_READY)
            {
                /* First call: initialize active modules */
                transdecode_master_selection();
                m_global_state = JpegState.DSTATE_RDCOEFS;
            }

            if (m_global_state == JpegState.DSTATE_RDCOEFS)
            {
                /* Absorb whole file into the coef buffer */
                for ( ; ; )
                {
                    ReadResult retcode;
                    /* Call progress monitor hook if present */
                    if (m_progress != null)
                        m_progress.Updated();

                    /* Absorb some more input */
                    retcode = m_inputctl.consume_input();
                    if (retcode == ReadResult.JPEG_SUSPENDED)
                        return null;

                    if (retcode == ReadResult.JPEG_REACHED_EOI)
                        break;

                    /* Advance progress counter if appropriate */
                    if (m_progress != null && (retcode == ReadResult.JPEG_ROW_COMPLETED || retcode == ReadResult.JPEG_REACHED_SOS))
                    {
                        m_progress.Pass_counter++;
                        if (m_progress.Pass_counter >= m_progress.Pass_limit)
                        {
                            /* startup underestimated number of scans; ratchet up one scan */
                            m_progress.Pass_limit += m_total_iMCU_rows;
                        }
                    }
                }

                /* Set state so that jpeg_finish_decompress does the right thing */
                m_global_state = JpegState.DSTATE_STOPPING;
            }

            /* At this point we should be in state DSTATE_STOPPING if being used
            * standalone, or in state DSTATE_BUFIMAGE if being invoked to get access
            * to the coefficients during a full buffered-image-mode decompression.
            */
            if ((m_global_state == JpegState.DSTATE_STOPPING || m_global_state == JpegState.DSTATE_BUFIMAGE) && m_buffered_image)
                return m_coef.GetCoefArrays();

            /* Oops, improper usage */
            ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)m_global_state);
            /* keep compiler happy */
            return null;
        }

        /// <summary>
        /// Initializes the compression object with default parameters, then copy from the source object 
        /// all parameters needed for lossless transcoding.
        /// </summary>
        /// <param name="dstinfo">Target JPEG compression object.</param>
        /// <remarks>Parameters that can be varied without loss (such as scan script and 
        /// Huffman optimization) are left in their default states.</remarks>
        public void jpeg_copy_critical_parameters(jpeg_compress_struct dstinfo)
        {
            /* Safety check to ensure start_compress not called yet. */
            if (dstinfo.m_global_state != JpegState.CSTATE_START)
                ERREXIT(J_MESSAGE_CODE.JERR_BAD_STATE, (int)dstinfo.m_global_state);

            /* Copy fundamental image dimensions */
            dstinfo.m_image_width = m_image_width;
            dstinfo.m_image_height = m_image_height;
            dstinfo.m_input_components = m_num_components;
            dstinfo.m_in_color_space = m_jpeg_color_space;

            /* Initialize all parameters to default values */
            dstinfo.jpeg_set_defaults();
            
            /* jpeg_set_defaults may choose wrong colorspace, eg YCbCr if input is RGB.
            * Fix it to get the right header markers for the image colorspace.
            */
            dstinfo.jpeg_set_colorspace(m_jpeg_color_space);
            dstinfo.m_data_precision = m_data_precision;
            dstinfo.m_CCIR601_sampling = m_CCIR601_sampling;
            
            /* Copy the source's quantization tables. */
            for (int tblno = 0; tblno < JpegConstants.NUM_QUANT_TBLS; tblno++)
            {
                if (m_quant_tbl_ptrs[tblno] != null)
                {
                    if (dstinfo.m_quant_tbl_ptrs[tblno] == null)
                        dstinfo.m_quant_tbl_ptrs[tblno] = new JQUANT_TBL();

                    Buffer.BlockCopy(m_quant_tbl_ptrs[tblno].quantval, 0,
                        dstinfo.m_quant_tbl_ptrs[tblno].quantval, 0,
                        dstinfo.m_quant_tbl_ptrs[tblno].quantval.Length * sizeof(short));

                    dstinfo.m_quant_tbl_ptrs[tblno].Sent_table = false;
                }
            }
            
            /* Copy the source's per-component info.
            * Note we assume jpeg_set_defaults has allocated the dest comp_info array.
            */
            dstinfo.m_num_components = m_num_components;
            if (dstinfo.m_num_components < 1 || dstinfo.m_num_components> JpegConstants.MAX_COMPONENTS)
                ERREXIT(J_MESSAGE_CODE.JERR_COMPONENT_COUNT, dstinfo.m_num_components, JpegConstants.MAX_COMPONENTS);

            for (int ci = 0; ci < dstinfo.m_num_components; ci++)
            {
                dstinfo.Component_info[ci].Component_id = m_comp_info[ci].Component_id;
                dstinfo.Component_info[ci].H_samp_factor = m_comp_info[ci].H_samp_factor;
                dstinfo.Component_info[ci].V_samp_factor = m_comp_info[ci].V_samp_factor;
                dstinfo.Component_info[ci].Quant_tbl_no = m_comp_info[ci].Quant_tbl_no;

                /* Make sure saved quantization table for component matches the qtable
                * slot.  If not, the input file re-used this qtable slot.
                * IJG encoder currently cannot duplicate this.
                */
                int tblno = dstinfo.Component_info[ci].Quant_tbl_no;
                if (tblno < 0 || tblno >= JpegConstants.NUM_QUANT_TBLS || m_quant_tbl_ptrs[tblno] == null)
                    ERREXIT(J_MESSAGE_CODE.JERR_NO_QUANT_TABLE, tblno);

                JQUANT_TBL c_quant = m_comp_info[ci].quant_table;
                if (c_quant != null)
                {
                    JQUANT_TBL slot_quant = m_quant_tbl_ptrs[tblno];
                    for (int coefi = 0; coefi < JpegConstants.DCTSIZE2; coefi++)
                    {
                        if (c_quant.quantval[coefi] != slot_quant.quantval[coefi])
                            ERREXIT(J_MESSAGE_CODE.JERR_MISMATCHED_QUANT_TABLE, tblno);
                    }
                }
                /* Note: we do not copy the source's Huffman table assignments;
                * instead we rely on jpeg_set_colorspace to have made a suitable choice.
                */
            }

            /* Also copy JFIF version and resolution information, if available.
            * Strictly speaking this isn't "critical" info, but it's nearly
            * always appropriate to copy it if available.  In particular,
            * if the application chooses to copy JFIF 1.02 extension markers from
            * the source file, we need to copy the version to make sure we don't
            * emit a file that has 1.02 extensions but a claimed version of 1.01.
            * We will *not*, however, copy version info from mislabeled "2.01" files.
            */
            if (m_saw_JFIF_marker)
            {
                if (m_JFIF_major_version == 1)
                {
                    dstinfo.m_JFIF_major_version = m_JFIF_major_version;
                    dstinfo.m_JFIF_minor_version = m_JFIF_minor_version;
                }

                dstinfo.m_density_unit = m_density_unit;
                dstinfo.m_X_density = (short)m_X_density;
                dstinfo.m_Y_density = (short)m_Y_density;
            }
        }

        /// <summary>
        /// Aborts processing of a JPEG decompression operation.
        /// </summary>
        /// <seealso cref="jpeg_common_struct.jpeg_abort"/>
        public void jpeg_abort_decompress()
        {
            jpeg_abort();
        }

        /// <summary>
        /// Sets processor for special marker.
        /// </summary>
        /// <param name="marker_code">The marker code.</param>
        /// <param name="routine">The processor.</param>
        /// <remarks>Allows you to supply your own routine to process 
        /// COM and/or APPn markers on-the-fly as they are read.
        /// </remarks>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public void jpeg_set_marker_processor(int marker_code, jpeg_marker_parser_method routine)
        {
            m_marker.jpeg_set_marker_processor(marker_code, routine);
        }

        /// <summary>
        /// Control saving of COM and APPn markers into <see cref="jpeg_decompress_struct.Marker_list">Marker_list</see>.
        /// </summary>
        /// <param name="marker_code">The marker type to save (see JPEG_MARKER enumeration).<br/>
        /// To arrange to save all the special marker types, you need to call this 
        /// routine 17 times, for COM and APP0-APP15 markers.</param>
        /// <param name="length_limit">If the incoming marker is longer than <c>length_limit</c> data bytes, 
        /// only <c>length_limit</c> bytes will be saved; this parameter allows you to avoid chewing up memory 
        /// when you only need to see the first few bytes of a potentially large marker. If you want to save 
        /// all the data, set <c>length_limit</c> to 0xFFFF; that is enough since marker lengths are only 16 bits. 
        /// As a special case, setting <c>length_limit</c> to 0 prevents that marker type from being saved at all. 
        /// (That is the default behavior, in fact.)
        /// </param>
        /// <seealso cref="jpeg_decompress_struct.Marker_list"/>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public void jpeg_save_markers(int marker_code, int length_limit)
        {
            m_marker.jpeg_save_markers(marker_code, length_limit);
        }

        /// <summary>
        /// Determine whether merged upsample/color conversion should be used.
        /// CRUCIAL: this must match the actual capabilities of merged upsampler!
        /// </summary>
        internal bool use_merged_upsample()
        {
            /* Merging is the equivalent of plain box-filter upsampling */
            if (m_do_fancy_upsampling || m_CCIR601_sampling)
                return false;

            /* my_upsampler only supports YCC=>RGB color conversion */
            if (m_jpeg_color_space != J_COLOR_SPACE.JCS_YCbCr || m_num_components != 3 ||
                m_out_color_space != J_COLOR_SPACE.JCS_RGB || m_out_color_components != JpegConstants.RGB_PIXELSIZE)
            {
                return false;
            }

            /* and it only handles 2h1v or 2h2v sampling ratios */
            if (m_comp_info[0].H_samp_factor != 2 || m_comp_info[1].H_samp_factor != 1 ||
                m_comp_info[2].H_samp_factor != 1 || m_comp_info[0].V_samp_factor > 2 ||
                m_comp_info[1].V_samp_factor != 1 || m_comp_info[2].V_samp_factor != 1)
            {
                return false;
            }

            /* furthermore, it doesn't work if we've scaled the IDCTs differently */
            if (m_comp_info[0].DCT_scaled_size != m_min_DCT_scaled_size ||
                m_comp_info[1].DCT_scaled_size != m_min_DCT_scaled_size ||
                m_comp_info[2].DCT_scaled_size != m_min_DCT_scaled_size)
            {
                return false;
            }

            /* ??? also need to test for upsample-time rescaling, when & if supported */
            /* by golly, it'll work... */
            return true;
        }

        /// <summary>
        /// Initialization of JPEG compression objects.
        /// The error manager must already be set up (in case memory manager fails).
        /// </summary>
        private void initialize()
        {
            /* Zero out pointers to permanent structures. */
            m_progress = null;
            m_src = null;

            for (int i = 0; i < JpegConstants.NUM_QUANT_TBLS; i++)
                m_quant_tbl_ptrs[i] = null;

            for (int i = 0; i < JpegConstants.NUM_HUFF_TBLS; i++)
            {
                m_dc_huff_tbl_ptrs[i] = null;
                m_ac_huff_tbl_ptrs[i] = null;
            }

            /* Initialize marker processor so application can override methods
            * for COM, APPn markers before calling jpeg_read_header.
            */
            m_marker_list = new List<jpeg_marker_struct>();
            m_marker = new jpeg_marker_reader(this);

            /* And initialize the overall input controller. */
            m_inputctl = new jpeg_input_controller(this);

            /* OK, I'm ready */
            m_global_state = JpegState.DSTATE_START;
        }

        /// <summary>
        /// Master selection of decompression modules for transcoding (that is, reading 
        /// raw DCT coefficient arrays from an input JPEG file.)
        /// This substitutes for initialization of the full decompressor.
        /// </summary>
        private void transdecode_master_selection()
        {
            /* This is effectively a buffered-image operation. */
            m_buffered_image = true;

            if (m_progressive_mode)
                m_entropy = new phuff_entropy_decoder(this);
            else
                m_entropy = new huff_entropy_decoder(this);

            /* Always get a full-image coefficient buffer. */
            m_coef = new jpeg_d_coef_controller(this, true);

            /* Initialize input side of decompressor to consume first scan. */
            m_inputctl.start_input_pass();

            /* Initialize progress monitoring. */
            if (m_progress != null)
            {
                int nscans = 1;
                /* Estimate number of scans to set pass_limit. */
                if (m_progressive_mode)
                {
                    /* Arbitrarily estimate 2 interleaved DC scans + 3 AC scans/component. */
                    nscans = 2 + 3 * m_num_components;
                }
                else if (m_inputctl.HasMultipleScans())
                {
                    /* For a nonprogressive multiscan file, estimate 1 scan per component. */
                    nscans = m_num_components;
                }

                m_progress.Pass_counter = 0;
                m_progress.Pass_limit = m_total_iMCU_rows * nscans;
                m_progress.Completed_passes = 0;
                m_progress.Total_passes = 1;
            }
        }

        /// <summary>
        /// Set up for an output pass, and perform any dummy pass(es) needed.
        /// Common subroutine for jpeg_start_decompress and jpeg_start_output.
        /// Entry: global_state = DSTATE_PRESCAN only if previously suspended.
        /// Exit: If done, returns true and sets global_state for proper output mode.
        ///       If suspended, returns false and sets global_state = DSTATE_PRESCAN.
        /// </summary>
        private bool output_pass_setup()
        {
            if (m_global_state != JpegState.DSTATE_PRESCAN)
            {
                /* First call: do pass setup */
                m_master.prepare_for_output_pass();
                m_output_scanline = 0;
                m_global_state = JpegState.DSTATE_PRESCAN;
            }

            /* Loop over any required dummy passes */
            while (m_master.IsDummyPass())
            {
                 /* Crank through the dummy pass */
                while (m_output_scanline < m_output_height)
                {
                    int last_scanline;
                    /* Call progress monitor hook if present */
                    if (m_progress != null)
                    {
                        m_progress.Pass_counter = m_output_scanline;
                        m_progress.Pass_limit = m_output_height;
                        m_progress.Updated();
                    }

                    /* Process some data */
                    last_scanline = m_output_scanline;
                    m_main.process_data(null, ref m_output_scanline, 0);
                    if (m_output_scanline == last_scanline)
                    {
                        /* No progress made, must suspend */
                        return false;
                    }
                }

                /* Finish up dummy pass, and set up for another one */
                m_master.finish_output_pass();
                m_master.prepare_for_output_pass();
                m_output_scanline = 0;
            }

            /* Ready for application to drive output pass through
            * jpeg_read_scanlines or jpeg_read_raw_data.
            */
            m_global_state = m_raw_data_out ? JpegState.DSTATE_RAW_OK : JpegState.DSTATE_SCANNING;
            return true;
        }

        /// <summary>
        /// Set default decompression parameters.
        /// </summary>
        private void default_decompress_parms()
        {
            /* Guess the input colorspace, and set output colorspace accordingly. */
            /* (Wish JPEG committee had provided a real way to specify this...) */
            /* Note application may override our guesses. */
            switch (m_num_components)
            {
                case 1:
                    m_jpeg_color_space = J_COLOR_SPACE.JCS_GRAYSCALE;
                    m_out_color_space = J_COLOR_SPACE.JCS_GRAYSCALE;
                    break;

                case 3:
                    if (m_saw_JFIF_marker)
                    {
                        /* JFIF implies YCbCr */
                        m_jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
                    }
                    else if (m_saw_Adobe_marker)
                    {
                        switch (m_Adobe_transform)
                        {
                            case 0:
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_RGB;
                                break;
                            case 1:
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
                                break;
                            default:
                                WARNMS(J_MESSAGE_CODE.JWRN_ADOBE_XFORM, m_Adobe_transform);
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr; /* assume it's YCbCr */
                                break;
                        }
                    }
                    else
                    {
                        /* Saw no special markers, try to guess from the component IDs */
                        int cid0 = m_comp_info[0].Component_id;
                        int cid1 = m_comp_info[1].Component_id;
                        int cid2 = m_comp_info[2].Component_id;

                        if (cid0 == 1 && cid1 == 2 && cid2 == 3)
                        {
                            /* assume JFIF w/out marker */
                            m_jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
                        }
                        else if (cid0 == 82 && cid1 == 71 && cid2 == 66)
                        {
                            /* ASCII 'R', 'G', 'B' */
                            m_jpeg_color_space = J_COLOR_SPACE.JCS_RGB;
                        }
                        else
                        {
                            TRACEMS(1, J_MESSAGE_CODE.JTRC_UNKNOWN_IDS, cid0, cid1, cid2);
                            /* assume it's YCbCr */
                            m_jpeg_color_space = J_COLOR_SPACE.JCS_YCbCr;
                        }
                    }
                    /* Always guess RGB is proper output colorspace. */
                    m_out_color_space = J_COLOR_SPACE.JCS_RGB;
                    break;

                case 4:
                    if (m_saw_Adobe_marker)
                    {
                        switch (m_Adobe_transform)
                        {
                            case 0:
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_CMYK;
                                break;
                            case 2:
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_YCCK;
                                break;
                            default:
                                WARNMS(J_MESSAGE_CODE.JWRN_ADOBE_XFORM, m_Adobe_transform);
                                /* assume it's YCCK */
                                m_jpeg_color_space = J_COLOR_SPACE.JCS_YCCK;
                                break;
                        }
                    }
                    else
                    {
                        /* No special markers, assume straight CMYK. */
                        m_jpeg_color_space = J_COLOR_SPACE.JCS_CMYK;
                    }

                    m_out_color_space = J_COLOR_SPACE.JCS_CMYK;
                    break;

                default:
                    m_jpeg_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
                    m_out_color_space = J_COLOR_SPACE.JCS_UNKNOWN;
                    break;
            }

            /* Set defaults for other decompression parameters. */
            m_scale_num = 1;       /* 1:1 scaling */
            m_scale_denom = 1;
            m_buffered_image = false;
            m_raw_data_out = false;
            m_dct_method = JpegConstants.JDCT_DEFAULT;
            m_do_fancy_upsampling = true;
            m_do_block_smoothing = true;
            m_quantize_colors = false;

            /* We set these in case application only sets quantize_colors. */
            m_dither_mode = J_DITHER_MODE.JDITHER_FS;
            m_two_pass_quantize = true;
            m_desired_number_of_colors = 256;
            m_colormap = null;

            /* Initialize for no mode change in buffered-image mode. */
            m_enable_1pass_quant = false;
            m_enable_external_quant = false;
            m_enable_2pass_quant = false;
        }

        private void jpeg_consume_input_start()
        {
            /* Start-of-datastream actions: reset appropriate modules */
            m_inputctl.reset_input_controller();

            /* Initialize application's data source module */
            m_src.init_source();
            m_global_state = JpegState.DSTATE_INHEADER;
        }

        private ReadResult jpeg_consume_input_inHeader()
        {
            ReadResult retcode = m_inputctl.consume_input();
            if (retcode == ReadResult.JPEG_REACHED_SOS)
            {
                /* Found SOS, prepare to decompress */
                /* Set up default parameters based on header data */
                default_decompress_parms();

                /* Set global state: ready for start_decompress */
                m_global_state = JpegState.DSTATE_READY;
            }

            return retcode;
        }
    }
}
