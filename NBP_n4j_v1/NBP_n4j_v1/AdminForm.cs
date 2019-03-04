using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neo4jClient;
using NBP_n4j_v1.DomainModel;
namespace NBP_n4j_v1
{
    public partial class AdminForm : Form
    {
        private List<String> types;
        private GraphClient client;
        private BindingSource bs = new BindingSource();
        public AdminForm()
        {
            InitializeComponent();
        }
        public AdminForm(GraphClient client)
        {
            InitializeComponent();
            this.client = client;
        }
        private void LoadTypes()
        {
            try { 
                this.types = this.client.Cypher.Match("(t:ProductType)")
                                               .Return<String>("t.name")
                                               .Results
                                               .ToList<String>();
                this.lbTypes.DataSource = null;
                this.lbTypes.DataSource = this.types;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            String newType = this.txtNewType.Text.Trim(' ');
            this.types.Add(newType);
            this.lbTypes.DataSource = null;
            this.lbTypes.DataSource = this.types;
            this.txtNewType.Text = "";
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            this.types = new List<String>();
            LoadTypes();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            foreach (var type in types)
            {
                this.client.Cypher.Merge("(pt:ProductType { name: '" + type + "' })").ExecuteWithoutResults();
            }
            MessageBox.Show("Product types defined successfuly");
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            foreach (int selectedIndex in this.lbTypes.SelectedIndices)
            {
                this.client.Cypher.Match("(type:ProductType { name: '" + types[selectedIndex] + "' })")
                                  .OptionalMatch("(type)-[r]-()")
                                  .Delete("type, r")
                                  .ExecuteWithoutResults();
            }
            MessageBox.Show("Successfully removed selected types");
            LoadTypes();
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < this.lbTypes.Items.Count; index++)
            {
                this.client.Cypher.Match("(type:ProductType { name: '" + types[index] + "' })")
                                  .OptionalMatch("(type)-[r]-()")
                                  .Delete("type, r")
                                  .ExecuteWithoutResults();
            }
            MessageBox.Show("Successfully removed types");
            LoadTypes();
        }
    }
}
