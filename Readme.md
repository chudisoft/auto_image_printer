# Auto Photo Printer

## Overview

Auto Photo Printer is a C#-based Console designed to automatically detect new JPEG or JPG images added to a specified folder, convert them to a printable PDF, and send them to the default printer. This script is tailored for Windows OS and is ideal for scenarios where automated image printing is required.

This project also provides C# Console that captures a photo from a camera, saves it, and prints the image in a specified position on an A4 paper. This is particularly useful for printing photos on pre-printed templates, such as newspaper-style layouts.

## Features

- **Automatic Image Detection**: The script monitors a specified folder and automatically processes new `.jpg` or `.jpeg` files added to the folder.
- **Image-to-PDF Conversion**: Detected images are converted to a PDF format, positioned at specified coordinates on an A4-sized page.
- **Automatic Printing**: The generated PDF is automatically sent to the default printer.
- **Customizable Output**: Users can specify the position, width, and height of the image on the PDF.
- **Default Folder Configuration**: The default folder for image monitoring is set to an `images` directory within the script's directory, which is created automatically if it doesn't exist.
- **User-Friendly Controls**: The script provides clear instructions on how to terminate the process (`Ctrl+C`), and can be customized by the user for different coordinates and image sizes.

## Requirements

- Visual Studio
- .net 4.7.2

## Installation

1. Install GhostScript from:
```https://ghostscript.com/releases/```

2. Clone the repository:

```
    git clone https://github.com/chudisoft/auto_image_printer.git
    cd auto_image_printer
```

3. Open project or solution file with Visual Studio



# PDF Coordinate System for Image Placement

In a PDF file, the coordinate system starts from the bottom-left corner, meaning:

- **X position** is measured from the left edge of the page.
- **Y position** is measured from the bottom edge of the page.

### A4 Page Dimensions

- **Width:** 595 points
- **Height:** 842 points

### Positioning the Image

To place the image at the very top of the page, set the `y` value close to 842 points, minus the height of the image. 

**Calculation for Y Value:**

If you want the top edge of the image to start exactly at the top of the page, you can calculate the `y` value as follows:

```
    y = 842 - image_height
```

This ensures that the top edge of the image aligns with the top edge of the page.

### Example:

If your image height is 200 points, then:

```
    y = 842 - 200 = 642
```

So, setting y = 642 will place the image such that its top edge aligns with the top of the page.

### Adjusting for Margins

If you want to add a margin from the top, reduce the y value slightly. For example, to add a 10-point margin:

```
    y = 842 - 200 - 10 = 632
```

### Recap

- y = 0: Image bottom edge aligns with the bottom of the page.
- y = 842: Image bottom edge aligns with the top of the page (image is off-page).
- y = 842 - image_height: Image top edge aligns with the top of the page.

Set your y value accordingly based on the image size and desired position.
