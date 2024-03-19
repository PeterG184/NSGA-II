using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NSGA_II
{
    static class Constants
    {
        public const int TREE_MAX_SIZE = 50;
        public const int TREE_MIN_SIZE = 5;
        public const int MUTATION_MAX_SIZE = 25;
        public const int MUTATION_CHANCE = 50;
        public const int POP_MAX_COUNT = 50;
        public const int GEN_MAX_COUNT = 10;
        public const int OPERATOR_CHANCE = 60; // Chance for a node to be an operator, otherwise it is a terminal. Higher values lead to longer trees generally
        public static readonly string[] functionSet = ["+", "-", "*", "/", "%"];
        public static readonly string[] restrictionSet = ["=", ">", "<", "!="];
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> selectedFunctions = [];
        private int _restrictionValue;

        public int RestrictionValue
        {
            get { return _restrictionValue; }
            set { _restrictionValue = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            foreach (string res in Constants.restrictionSet)
            {
                restrictionTypePicker.Items.Add(res);
            }

            restrictionTypePicker.SelectedIndex = 0;
            RestrictionValue = 0;

            FunctionsItemsControl.ItemsSource = Constants.functionSet;
        }

        private void RestrictionValuePreview(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        private static bool IsNumeric(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void AddRestrictionClicked(object sender, RoutedEventArgs e)
        {
            AddRestriction();
        }

        private void AddRestriction()
        {
            string op = Constants.restrictionSet[restrictionTypePicker.SelectedIndex];
            ObjectiveListBox.Items.Add(op + RestrictionValue.ToString());
            RestrictionValueInput.Clear();
        }

        private void FunctionCheckboxChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string content = checkBox.Content.ToString();

            if (checkBox.IsChecked == true)
            {
                selectedFunctions.Add(content);
            }
            else
            {
                selectedFunctions.Remove(content);
            }
        }
    }

    public class Program
    {
        private List<string> selectedFunctionSet = [];

        public Program()
        { 
            
        }

        public void SetSelectedFunctionSet(List<string> selectedFunctions)
        {
            this.selectedFunctionSet = selectedFunctions;
        }

        public List<string> getSelectedFunctionSet() 
        { 
            return selectedFunctionSet; 
        }
    }

    public class Node
    {
        public string op;
        public int val;
        public bool terminal;
        public int[] children = [];

        public Node(string opType, int value, bool isTerminal)
        {
            if (isTerminal)
            {
                op = "";
                val = value;
                terminal = true;
            }
            else
            {
                op = opType;
                val = 0;
                terminal = false;
            }
        }
    }

    public class Individual
    {
        public Node[] tree = [];
        public int result;
        public int[] dominates = [];
        public int rank;
        public double crowdingDistance;
        public int front = -1;

        private List<string> allowedFunctions = [];
        private Stack<Node> unterminatedNodes = [];

        public Individual(List<string> functionSet)
        {
            allowedFunctions = functionSet;
            AddNode();
            while (unterminatedNodes.Count > 0)
            {
                Node parentNode = unterminatedNodes.Pop();
                int parentNodePos = Array.IndexOf(tree, parentNode);
                if (!tree[parentNodePos].terminal)
                {
                    // Need to add two children to fill the tree
                    AddNode(parentNodePos);
                    AddNode(parentNodePos);
                }
            }
        }

        private void AddNode()
        {
            Node newNode = CreateNewNode();
            tree.Append(newNode);
            unterminatedNodes.Append(tree[0]);
        }

        private void AddNode(int parentNodePos)
        {
            Node newNode = CreateNewNode();
            tree[parentNodePos].children.Append(tree.Length);
            tree.Append(newNode);
            unterminatedNodes.Append(tree[tree.Length - 1]);
        }

        private Node CreateNewNode()
        {
            Random random = new();
            
            if (random.Next(0, 100) < Constants.OPERATOR_CHANCE)
            {
                Node newNode = new(allowedFunctions[random.Next(0, allowedFunctions.Count - 1)], 0, true);
                return newNode;
            }
            else
            {
                Node newNode = new("", random.Next(), true);
                return newNode;
            }
        }
    }

    public class Population
    {
        public Individual[] populationList = [];
        public int[] dominatedIndividuals = [];
        public int dominationCount = 0;

        public Population(List<string> functionSet)
        {
            while (populationList.Length != Constants.POP_MAX_COUNT)
            {
                Individual ind = new Individual(functionSet);
                populationList.Append(ind);
            }
            Console.WriteLine(populationList);
        }
    }

}