using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Puzzle
{
    public partial class PuzzleUI : Form
    {
        private Strategy strategy;
        private Heuristic heuristic;
        private LinearShuffle<int> shuffle;
        private WindowsFormsSynchronizationContext syncContext;
        Dictionary<int, Button> uiButtons;
        private int[] initialState;
        private bool busy;

        public PuzzleUI()
        {
            InitializeComponent();
            syncContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;

            Initialize();
        }

        private void Initialize()
        {
            initialState = new int[] { 8, 7, 2, 4, 6, 3, 1, -1, 5 };

            shuffle = new LinearShuffle<int>();
            strategy = new Strategy();
            heuristic = Heuristic.ManhattanDistance;
            strategy.OnStateChanged += OnStrategyStateChanged;
            strategy.OnPuzzleSolved += OnPuzzleSolved;

            uiButtons = new Dictionary<int, Button>();
            uiButtons[0] = button1;
            uiButtons[1] = button2;
            uiButtons[2] = button3;
            uiButtons[3] = button4;
            uiButtons[4] = button5;
            uiButtons[5] = button6;
            uiButtons[6] = button7;
            uiButtons[7] = button8;
            uiButtons[8] = button9;

            DisplayState(initialState, false);

            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;
        }

        private void SwapValues(int x, int y)
        {
            int temp = initialState[x];
            initialState[x] = initialState[y];
            initialState[y] = temp;
        }

        private void OnStrategyStateChanged(int[] state, bool isFinal)
        {
            syncContext.Post(item => DisplayState(state, isFinal), null);
            Thread.Sleep(1500);
        }

        private void OnPuzzleSolved(int steps, int time, int statesExamined)
        {
            Action action = () =>
                {
                    progressBar.Visible = false;
                    this.Cursor = Cursors.Default;

                    if (steps > -1)
                    {
                        statusLabel.Text = "Steps: " + steps.ToString("n0");
                        MessageBox.Show(this, "Solution found!");
                    }
                    else
                    {
                        statusLabel.Text = "Steps: none";
                        MessageBox.Show(this, "No solution found!");
                    }
                };

            syncContext.Send(item => action.Invoke(), null);
        }

        private void DisplayState(int[] nodes, bool isFinal)
        {
            if (nodes != null)
            {
                this.gameUI.SuspendLayout();

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] > 0)
                    {
                        uiButtons[i].Text = nodes[i].ToString();
                    }
                    else
                    {
                        uiButtons[i].Text = null;
                    }
                }

                this.gameUI.ResumeLayout();
            }

            if (isFinal)
            {
                busy = false;
                buttonShuffle.Enabled = true;
                buttonStart.Enabled = true;
            }
        }

        private void StartSolvingPuzzle()
        {
            strategy.Solve(initialState, heuristic);

            progressBar.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            statusLabel.Text = "Finding solution...";
            busy = true;
        }

        private bool isBusy()
        {
            return !busy;
        }

        private void ShuffleButton_Click(object sender, EventArgs e)
        {
            if (isBusy())
            {
                shuffle.Shuffle(initialState);
                // Display state
                DisplayState(initialState, false);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (isBusy())
            {
                StartSolvingPuzzle();
            }
        }

        private void ShuffleMenu_Click(object sender, EventArgs e)
        {
            if (isBusy())
            {
                shuffle.Shuffle(initialState);
                // Display state
                DisplayState(initialState, false);
            }
        }

        private void SolveMenu_Click(object sender, EventArgs e)
        {
            if (isBusy())
            {
                StartSolvingPuzzle();
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
