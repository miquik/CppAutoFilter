using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CppAutoFilter
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MainWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("416919de-3777-471a-8521-33947b0f7546");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MainWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MainWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MainToolWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MainWindowCommand(package, commandService);
        }


        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            /*
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(MainWindowControl), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }
            */
            MainWindowControl window = new MainWindowControl();

            package.JoinableTaskFactory.Run(async delegate
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                var dteobj = await package.GetServiceAsync(typeof(EnvDTE.DTE));
                var dte = dteobj as EnvDTE.DTE;
                if (dte == null)
                {
                    throw new NotSupportedException("Cannot get DTE");
                }
                Array projects = dte.ActiveSolutionProjects as Array;
                if (projects != null && projects.Length != 0)
                {

                    EnvDTE.Project selectedProject = projects.GetValue(0) as EnvDTE.Project;
                    MainWindowControl mainToolWindow = (MainWindowControl)window;
                    mainToolWindow.UpdateProjectInfo(selectedProject);
                }
                else
                {
                    throw new NotSupportedException("No Project in solution or selected");
                }
  
            });
            window.Show();
            // IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            // Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
           //  Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(window.Show());
        }

        /*
        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            // ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(MainToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dteobj = await package.GetServiceAsync(typeof(EnvDTE.DTE));
            var dte = dteobj as EnvDTE.DTE;
            if (dte == null)
            {
                throw new NotSupportedException("Cannot get DTE");
            }
            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects != null && projects.Length != 0)
            {

                EnvDTE.Project selectedProject = projects.GetValue(0) as EnvDTE.Project;
                MainToolWindow mainToolWindow = (MainToolWindow)window;
                mainToolWindow.Project = selectedProject;
            }
            else
            {
                throw new NotSupportedException("No Project in solution or selected");
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        */
    }
}
