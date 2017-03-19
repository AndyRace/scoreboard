﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreboardTest.ViewModels
{
    class ShellViewDesigntimeModel : Screen
    {
        public ObservableCollection<DebugItemViewModel> DebugInfo { get; set; }

        public ShellViewDesigntimeModel()
        {
            DebugInfo = new ObservableCollection<DebugItemViewModel>();

            var rnd = new Random();
            for (var i = 0; i < 100; i++)
            {
                switch (rnd.Next(3))
                {
                    case 0:
                        DebugInfo.Add(new DebugItemViewModel(DateTime.Now, "INFO", $"This is some info {i}"));
                        break;
                    case 1:
                        DebugInfo.Add(new DebugItemViewModel(DateTime.Now, "WARN", $"This is a warning {i}"));
                        break;
                    case 2:
                        DebugInfo.Add(new DebugItemViewModel(DateTime.Now, "ERROR", $"This is an error {i}"));
                        break;
                }
            }
        }

        public bool CanRunTest => false;

        public string Info
        {
            get
            {
                return "dt: Test";
            }
        }

        public string Value => "dt: Value...";
    }
}
