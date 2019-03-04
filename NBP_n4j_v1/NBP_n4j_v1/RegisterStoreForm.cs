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
    public partial class RegisterStoreForm : Form
    {
        private GraphClient client;
        public Store registeredStore;
        public RegisterStoreForm()
        {
            InitializeComponent();
        }
        public RegisterStoreForm(GraphClient client)
        {
            InitializeComponent();
            this.client = client;
        }
        private void btnRegister_Click(object sender, EventArgs e)
        {
            List<Store> storesWithName = this.client.Cypher.Match("(store:Store { name: '" + this.txtName.Text + "' })" 
                                                                    + "-[r:LOCATED_AT { address: '" + this.txtAddress.Text+ "' }]->" 
                                                                    + "(loc:Location { name: '" + this.txtCity.Text + "' })")
                                                                    .Return<Store>("store")
                                                                    .Results
                                                                    .ToList();
            if (storesWithName.Count > 0)
            {
                MessageBox.Show("Store with name: " + this.txtName.Text 
                                + " on address: " + this.txtAddress.Text
                                + ", " + this.txtCity.Text + " alredy registered.");
                return;
            }

            this.client.Cypher.Merge("(store:Store { name: '" + this.txtName.Text 
                                    + "', password: '" + this.txtPassword.Text 
                                    + "', website: '" + this.txtWebsite.Text + "'})")
                                    .Merge("(loc:Location { name: '" + this.txtCity.Text + "'})")
                                    .Merge("(store)-[:LOCATED_AT { address: '" 
                                            + this.txtAddress.Text + "' }]->(loc)")
                                    .ExecuteWithoutResults();
            this.registeredStore = new Store(this.txtName.Text, this.txtPassword.Text, this.txtWebsite.Text);
            MessageBox.Show("Register successfull");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
