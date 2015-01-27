//The MIT License (MIT)

//Copyright (c) 2015 Tusk Software - http://tusksoft.com

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ResxNewlineStripper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = "Select the folder which contains the resource files you wish to strip of newlines";
            var dialogResult = dialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.SelectedPath;
                try
                {
                    var filePaths = Directory.GetFiles(path, "*.resx");
                    if (filePaths.Count() > 0)
                    {
                        var mbResult = MessageBox.Show("The following resx files' values will be stripped of newlines:\n\n" +
                                                       filePaths.Aggregate((current, next) => current + "\n" + next) +
                                                       "\n\nDo you want to proceed?",
                            "Confirm files to be stripped",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (mbResult == MessageBoxResult.Yes)
                        {
                            foreach (var filePath in filePaths)
                            {
                                bool currentlyReadingMultiLineValue = false;
                                StringBuilder result = new StringBuilder();
                                StringBuilder innerResult = null;
                                var lines = File.ReadAllLines(filePath);
                                for (var i = 0; i < lines.Length; i++)
                                {
                                    if (Regex.IsMatch(lines[i], @"<value>.*<\/value>"))
                                    {
                                        result.AppendLine(lines[i]);
                                    }
                                    else if (Regex.IsMatch(lines[i], @"<value>"))
                                    {
                                        currentlyReadingMultiLineValue = true;
                                        innerResult = new StringBuilder();
                                        if (!lines[i].EndsWith(@"<value>") && !lines[i].EndsWith(" "))
                                        {
                                            innerResult.Append(lines[i] + " ");
                                        }
                                        else
                                        {
                                            innerResult.Append(lines[i]);
                                        }
                                    }
                                    else if (Regex.IsMatch(lines[i], @"<\/value>"))
                                    {
                                        currentlyReadingMultiLineValue = false;
                                        result.AppendLine(innerResult.ToString().TrimEnd(' ') + lines[i]);
                                    }
                                    else if (currentlyReadingMultiLineValue)
                                    {
                                        if (lines[i].EndsWith(" "))
                                        {
                                            innerResult.Append(lines[i]);
                                        }
                                        else
                                        {
                                            innerResult.Append(lines[i] + " ");
                                        }
                                    }
                                    else if (i != lines.Length - 1)
                                    {
                                        result.AppendLine(lines[i]);
                                    }
                                    else
                                    {
                                        result.Append(lines[i]);
                                    }
                                }
                                File.WriteAllText(filePath, result.ToString());
                            }
                            MessageBox.Show("Resx values successfully stripped of newlines.", "Success",
                                MessageBoxButton.OK);
                        }
                        else
                        {
                            MessageBox.Show("Operation cancelled. No files were modified.", "Cancelled",
                                MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No resx files were found in the specified directory.", "No files found",
                            MessageBoxButton.OK);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while attempting to load the file. The error is:"
                                    + Environment.NewLine + ex + Environment.NewLine);
                }
            }
            Application.Current.Shutdown();
        }
    }
}
