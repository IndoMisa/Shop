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
    public partial class StoreForm : Form
    {
        private String storeName;
        private String password;
        private GraphClient client;
        private List<ProductType> typeList;
        public StoreForm()
        {
            InitializeComponent();
        }
        public StoreForm(GraphClient client, String storeName, String password)
        {
            InitializeComponent();
            this.client = client;
            this.storeName = storeName;
            this.password = password;
        }

        private void StoreForm_Load(object sender, EventArgs e)
        {
            InitializeTypeList();
        }
        private void InitializeTypeList()
        {
            this.typeList = this.client.Cypher.Match("(t:ProductType)")
                                                            .Return<ProductType>("t")
                                                            .Results
                                                            .ToList();
            LoadTypes();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void LoadTypes()
        {
            CheckBox newCheckBox;
            foreach (var type in typeList)
            {
                newCheckBox = new CheckBox();
                newCheckBox.Name = "cb" + type.name;
                newCheckBox.Text = type.name;
                this.flpTypes.Controls.Add(newCheckBox);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateEntry())
            {
                return;
            }
            try
            {
                List<Product> productsWithNameAndCompany = this.client.Cypher.Match("(product:Product)")
                                                                             .Where("toLower(product.name) = '" + this.txtName.Text.ToLower() + "'")
                                                                             .Match("(product)<-[:MAKES]-(company:Company)")
                                                                             .Where("toLower(company.name) = '" + this.txtCompany.Text.ToLower() + "'")
                                                                             .Return<Product>("product")
                                                                             .Results
                                                                             .ToList();
                if (productsWithNameAndCompany.Count == 0)
                {
                    this.client.Cypher.Merge("(p:Product { name: '" + this.txtName.Text + "'})")
                                      .Merge("(c:Company {name: '" + this.txtCompany.Text + "'})")
                                      .Merge("(c)-[:MAKES]->(p)")
                                      .ExecuteWithoutResults();
                }

                this.client.Cypher.Match("(store:Store { name: '" + this.storeName + "'})")
                                  .Match("(p:Product)")
                                  .Where("toLower(p.name) = '" + this.txtName.Text.ToLower() + "'")
                                  .Merge("(store)-[rel:SELLS]->(p)")
                                  .Set("rel.price = " + this.numPrice.Value.ToString()
                                        + ", rel.link = '" + this.txtLink.Text + "'")
                                  .ExecuteWithoutResults();

                foreach (var item in this.flpTypes.Controls)
                {
                    CheckBox checkBox = (CheckBox)item;
                    if (checkBox.Checked)
                    {
                        this.client.Cypher.Match("(p:Product)")
                                          .Where("toLower(p.name) = '" + this.txtName.Text.ToLower() + "'")
                                          .Match("(type:ProductType { name: '" + checkBox.Text + "'})")
                                          .Merge("(p)-[:BELONGS_TO]->(type)")
                                          .ExecuteWithoutResults();
                    }
                }
                MessageBox.Show("Successfully registered a product");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private bool ValidateEntry()
        {
            txtName.Text = txtName.Text.Trim(' ');
            txtCompany.Text = txtCompany.Text.Trim(' ');
            txtLink.Text = txtLink.Text.Trim(' ');
            if ( txtName.Text == "")
            {
                MessageBox.Show("Field 'Name' requiered");
                return false;
            }
            if ( txtCompany.Text == "")
            {
                MessageBox.Show("Field 'Company name' required");
                return false;
            }
            if (txtLink.Text == "")
            {
                MessageBox.Show("Field 'Link to product' required");
                return false;
            }
            if (numPrice.Value == 0)
            {
                MessageBox.Show("Price must be greater then 0");
                return false;
            }
            bool flag = false;
            for (int i = 0; i < this.flpTypes.Controls.Count && !flag; i++)
            {
                CheckBox cb = (CheckBox)this.flpTypes.Controls[i];
                if (cb.Checked)
                    flag = true;
            }
            if (!flag)
            {
                MessageBox.Show("You must choose at least one type.");
                return false;
            }
            return true;
        }

        private void btnSubmit_Click_1(object sender, EventArgs e)
        {

        }
    }
}
