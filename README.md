# SkiaSharpError
Access Violation Error

Open the solution location in https://github.com/gktval/SkiaSharpError/tree/main/SkiaSharpAccessViolation

There are 3 projects in the solution:
1) SkiaSharpAccessViolation.csproj (This is a .Net Maui Project). It should be configured to run Windows platform
2) ProjNET (This is an open source project that is included as a dependency)
3) ProjNet.Sqlite (This is an open source project that uses sqlite)

SkiaSharpAccessViolation.csproj relies on two dependencies - CommunityToolkit.Maui and SkiaSharp.Views.Maui.Controls

# Steps to reproduce errer
1) Start SkiaSharpAccessViolation with Windows. Click the "Click me" button.
2) A dialog will display. Click 4326 WGS-84 projection, then click OK.
3) An image should display on the screen.

Repeat steps 1-3 one more time. When you click OK on the Coordinate System popup, an AccessViolation error will be thrown.

# Notes
In my original project (not this one), the error is thrown the first time the code is ran through. I am not quite sure why this one must be called twice.
I have had the problem occur with both Windows and Android.

I have tried using a simple DisplayAlert instead of the Popup, but I could not reproduce the error. Maybe do to further multithread calls...?
