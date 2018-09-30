using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullzipPDFTools.converter
{
    /// <summary>
    /// http://www.biopdf.com/guide/settings.php
    /// </summary>
    public enum BullzipPDFOptions
    {
        //PDF Printer Settings
        Output,
        ConfirmOverwrite, //Determines if the user has to confirm the overwrite of an existing PDF file.
        ConfirmNewFolder, //Warn the user before creating new folders if the output file name contains folders that does not exist yet.
        ShowSaveAS, //When should the "Save AS" dialog be shown? If you use the "nofile" setting then the dialog is only shown if no output setting is specified.
        ShowSettings, //This setting determines if the user will see the settings dialog when a PDF is created.
        ShowPDF, //Specifies if the resulting PDF file should be shown after generation
        OpenFolder, //Setting openfolder=yes will open the output folder in the file explorer when the conversion is done.
        AfterPrintProgram,
        AfterPrintProgramDir,
        AfterPrintProgramMode, //The mode is used to control if the program is started as hidden, normal, minimized, or maximized.
        RunOnSuccess,
        RunOnSuccessDir,
        RunOnSuccessMode,
        RunOnError,
        RunOnErrorDir,
        RunOnErrorMode,
        StatusFile, //The full path of a file name used as a status file. A status file is created after the print job. This file holds information about the last operation.
        StatusFileEncoding,
        ShowProgress, //Using this setting you can optionally disable the progress notification in the system tray. By default the progress indicator is shown.
        ShowProgressFinished, //The current version shows a balloon tip when the document is created. You can disable this tip by setting this value to no.
        DisableOptionDialog,
        GhostscriptTimeout,
        RememberLastFileName, //Remember the last file name specified by the user.
        RememberLastFolderName, //RememberLastFolderName
        SuppressErrors, //If you choose to suppress errors then the dialog showing error information will not be shown. This is particular useful if you share the printer or use the RunOnError setting.
        MacroDir,
        LicenseFile,
        LicenseData,
        ExtractText,
        TextFileName,
        ImageCompression, //By default the images in your PDF document will be compressed to make the resulting document smaller in size. This compressiona can be turned on and off with this setting
        ImageCompressionType,
        ImageCompressionLevel,
        ImageCompressionQuality,
        MergeFile,
        MergePosition,
        Superimpose,
        SuperimposeLayer,
        SuperimposeResolution,
        //PDF Document Settings
        Target, //The target device sets the quality of the PDF document. Better quality results in larger PDF files. Available targets are screen, ebook, printer, prepress and default. Please note that it is case sensitive. Specifying an invalid value will most likely result in an empty pdf file.
        Author,
        UseDefaultAuthor,
        Title,
        UseDefaultTitle,
        Subject,
        Keywords,
        Creator,
        Producer,
        //PDF Display Settings
        Zoom, //Specifies the initial zoom factor of the PDF document when it is viewed. The value is the zoom factor in percent. If no zoom value is specified or the value is 0 (zero) then the initial view will fit the document to the window of the reader. Please note that not all viewers respect this setting.
        UseThumbs, //Determines if the initial view of the document will include a list of thumbnail images for all the pages in the document. 
        AutoRotatePages,
        Orientation,
        Linearize,
        //Image Creation Settings
        Device, //"bmpmono", "bmpgray", "bmpsep1", "bmpsep8", "bmp16", "bmp256", "bmp16m", "bmp32b", "epswrite", "pswrite", "psraw", "jpeg", "jpeggray", "pcxmono", "pcxgray", "pcx16", "pcx256", "pcx24b", "pcxcmyk", "pngmono", "pnggray", "png16", "png256", "png16m", "pngalpha", "tiffgray", "tiff12nc", "tiff24nc", "tiff32nc", "tiffsep", "tiffcrle", "tiffg3", "tiffg32d", "tiffg4", "tifflzw", "tiffpack", "pdfwrite", "docwrite"
        DeviceList,
        Res, //Resolution of image in dots per inch. Unless values are specified in ResX or ResY this values is used for both horizontal and vertical resolution. The default value is 150.
        ResX, //Horizontal resolution of image.
        ResY, //Vertical resolution of image.
        TextAlphaBits, //Determines the quality of antialiasing for text elements. values: 1, 2, 4 (default)
        GraphicsAlphaBits, //Determines the quality of antialiasing for graphics elements. values: 1, 2, 4 (default)
                           //Other Settings
        CompatibilityLevel, //PDF readers support different versions of the PDF format specification. You can use this setting to tell the printer to make your document compliant with a specific PDF version. values: 1.1, 1.2, 1.3, 1.4, 1.5 (default), 1.6, 1.7
        Format, //Use this setting to make the resulting PDF document PDF/A-1B compliant. The default value is blank. If you change the value to pdfa1b it will try to make the document PDF/A-1b compliant.
        WipeMethod,
        SharedOptionSetFolder,
        AppendIfExists, //Sometimes you would like to append to an existing output file if the specified output file already exists. With this setting you can make that the default behavior. The program will no longer prompt the user to overwrite an existing file if this setting is set to yes.
        CustomGui,
        EmbedAllFonts, //This setting will control if fonts are included in the resulting PDF document. When you embed the font the recipient will get the full font with the PDF document. If the recipients should be able to edit the PDF with a PDF editor then you should use this setting. Embedding fonts will increase the size of the PDF document.
        SubsetFonts,
        ColorModel, //Specify the color model you want the distiller to use when processing the print job. The color model may affect how specific colors in your print job will look.  values: RGB, CMYK, GRAY
        Distiller,
        DeleteOutput, //Sometimes it can be useful to delete the output after it is created. It may seem a bit strange but in scenarios where you use RunOnSuccess, RunOnError, or AfterPrintProgram, you may not need the output file anymore at this stage. 
                      //User Interface Settings
        HideTabs,
        HideOptionTabs
    }
}
