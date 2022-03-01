using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace CppAutoFilter
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("e5972d47-b1b1-414e-a364-1462b33aec25")]
    public class MainToolWindow : ToolWindowPane
    {
        private MainToolWindowControl mainToolWindowControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolWindow"/> class.
        /// </summary>
        public MainToolWindow() : base(null)
        {
            this.Caption = "MainToolWindow";

            mainToolWindowControl = new MainToolWindowControl();

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = mainToolWindowControl;
        }

        public void UpdateProjectInfo(EnvDTE.Project project)
        {
            mainToolWindowControl.UpdateProjectInfo(project);
        }
    }
}
