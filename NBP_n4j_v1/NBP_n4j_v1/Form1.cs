using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBP_n4j_v1.DomainModel;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace NBP_n4j_v1
{
    public partial class Form1 : Form
    {
        private GraphClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.cbType.SelectedIndex = 0;
            client = new GraphClient(new Uri("http://localhost:11011/db/data"), "neo4j", "root");
            try
            {
                client.Connect();
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (cbType.SelectedItem.ToString() == "User")
                this.loginUser();
            else if (cbType.SelectedItem.ToString() == "Store")
                this.loginStore();
            else if (cbType.SelectedItem.ToString() == "Admin")
                this.loginAdmin();

        }
        private void loginUser()
        {
            List<User> users = this.client.Cypher.Match("(user:User { email: '" + this.txtEmailOrStoreName.Text
                                                            + "', password: '" + this.txtPassword.Text + "' })")
                                                            .Return<User>("user")
                                                            .Results
                                                            .ToList<User>();
            if (users.Count == 0)
            {
                MessageBox.Show("Invalid email or password");
                return;
            }

            ClientForm clientForm = new ClientForm(this.client, users[0].email, users[0].password);
            this.Hide();
            clientForm.ShowDialog();
            this.Close();
        }
        private void loginStore()
        {
            List<Store> stores = this.client.Cypher.Match("(store:Store { name: '" + this.txtEmailOrStoreName.Text
                                                            + "', password: '" + this.txtPassword.Text + "' })")
                                                            .Return<Store>("store")
                                                            .Results
                                                            .ToList<Store>();
            if (stores.Count == 0)
            {
                MessageBox.Show("Invalid store name or password");
                return;
            }

            StoreForm storeForm = new StoreForm(this.client, stores[0].name, stores[0].password);
            this.Hide();
            storeForm.ShowDialog();
            this.Close();
        }
        private void loginAdmin()
        {
            List<User> users = this.client.Cypher.Match("(user:User { name: '" + this.txtEmailOrStoreName.Text
                                                            + "', password: '" + this.txtPassword.Text + "' })")
                                                            .Return<User>("user")
                                                            .Results
                                                            .ToList<User>();
            if (users.Count == 0)
            {
                MessageBox.Show("Invalid name or password");
                return;
            }

            AdminForm adminForm = new AdminForm(this.client);
            this.Hide();
            adminForm.ShowDialog();
            this.Close();
        }
        private void lblGoToRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.client.Dispose();
        }

        private void cbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbType.SelectedItem.ToString() == "User") {
                lblEmailOrUsername.Text = "Email:";
                lblGoToRegister.Text = "Sign up as user";
                lblGoToRegister.Show();
            }
            else if (cbType.SelectedItem.ToString() == "Store") { 
                lblEmailOrUsername.Text = "Store name:";
                lblGoToRegister.Text = "Sign up a store";
                lblGoToRegister.Show();
            }
            else if (cbType.SelectedItem.ToString() == "Admin")
            {
                lblEmailOrUsername.Text = "Name:";
                lblGoToRegister.Hide();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lblGoToRegister_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (cbType.SelectedItem.ToString() == "User")
            {
                RegisterForm registerForm = new RegisterForm(this.client);
                this.Hide();
                DialogResult res = registerForm.ShowDialog();
                if (res == DialogResult.OK)
                {
                    ClientForm clientForm = new ClientForm(this.client, registerForm.registeredUser.name, registerForm.registeredUser.password);
                    clientForm.ShowDialog();
                    this.Close();
                }
                this.Show();
            }
            else if (cbType.SelectedItem.ToString() == "Store")
            {
                RegisterStoreForm registerForm = new RegisterStoreForm(this.client);
                this.Hide();
                DialogResult res = registerForm.ShowDialog();
                if (res == DialogResult.OK)
                {
                    StoreForm storeForm = new StoreForm(this.client, registerForm.registeredStore.name, registerForm.registeredStore.password);
                    storeForm.ShowDialog();
                    this.Close();
                }
                this.Show();
            }
        }
    }
}