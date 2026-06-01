using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PizzeriaApp
{
    public partial class MainForm : Form
    {
        private Panel pnlAuth, pnlAdmin, pnlUser;
        private TextBox txtLogin, txtPassword;
        private Button btnLogin, btnResetCaptcha;
        private PictureBox[] pieces = new PictureBox[4];
        private PictureBox[] targets = new PictureBox[4];
        private int[] targetContents = new int[4];
        private Label lblCaptchaStatus;
        private int consecutiveFailures = 0;

        private DataGridView dgvProducts, dgvOrders, dgvProduction, dgvUsers;
        private TabControl tabControl;
        private TextBox txtSearchOrder, txtSearchProduct;
        private Button btnSearchOrder, btnSearchProduct, btnExportReport, btnAdminLogout;
        private Label lblWelcome;
        private Button btnUserLogout;

        private List<User> users;
        private List<Product> products;
        private List<Order> orders;
        private List<Production> productions;

        private string currentUser = "";
        private string currentRole = "";

        public MainForm()
        {
            this.Text = "ООО Два сеньора - Пиццерия";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadData();
            InitializeAuthPanel();
            InitializeAdminPanel();
            InitializeUserPanel();
            ShowAuthPanel();
        }

        private void LoadData()
        {
            users = new List<User>
            {
                new User { Login = "admin", Password = "123", Role = "Admin", IsBlocked = false, FullName = "Администратор" },
                new User { Login = "user", Password = "123", Role = "User", IsBlocked = false, FullName = "Пользователь" }
            };

            products = new List<Product>
            {
                new Product { Name = "Пицца \"Пеперони\" 33см.", Price = 850, Unit = "шт", CostPrice = 468.5m },
                new Product { Name = "Пицца \"Маргарита\" 33см.", Price = 710, Unit = "шт", CostPrice = 0 },
                new Product { Name = "Пицца \"Гавайская\" 30см.", Price = 420, Unit = "шт", CostPrice = 0 },
                new Product { Name = "Пицца \"Морская\" 30см.", Price = 640, Unit = "шт", CostPrice = 0 }
            };

            orders = new List<Order>
            {
                new Order
                {
                    Number = 2,
                    Date = new DateTime(2025, 6, 6),
                    Executor = "ООО \"Два сеньора\"",
                    CustomerName = "ООО \"Кинотеатр Квант\"",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { ProductName = "Пицца \"Пеперони\" 33см.", Quantity = 4, Unit = "шт", Price = 850, Sum = 3400 },
                        new OrderItem { ProductName = "Пицца \"Маргарита\" 33см.", Quantity = 6, Unit = "шт", Price = 710, Sum = 4260 }
                    },
                    Total = 7660
                }
            };

            productions = new List<Production>
            {
                new Production
                {
                    Number = 1,
                    Date = new DateTime(2025, 6, 9),
                    ProductName = "Пицца \"Пеперони\" 33см.",
                    Quantity = 1,
                    Unit = "шт",
                    Materials = new List<ProductionMaterial>
                    {
                        new ProductionMaterial { Name = "Куриное яйцо", Quantity = 2, Unit = "шт" },
                        new ProductionMaterial { Name = "Майонез", Quantity = 0.03m, Unit = "кг" },
                        new ProductionMaterial { Name = "Бекон", Quantity = 0.2m, Unit = "кг" },
                        new ProductionMaterial { Name = "Тесто", Quantity = 0.5m, Unit = "шт" },
                        new ProductionMaterial { Name = "Сыр чеддер", Quantity = 0.15m, Unit = "шт" },
                        new ProductionMaterial { Name = "Сыр пармезан", Quantity = 0.1m, Unit = "шт" },
                        new ProductionMaterial { Name = "Томаты черри", Quantity = 0.2m, Unit = "кг" }
                    }
                }
            };
        }

        private void InitializeAuthPanel()
        {
            pnlAuth = new Panel() { Dock = DockStyle.Fill, AutoScroll = true };

            Label lblTitle = new Label()
            {
                Text = "ООО Два сеньора - Вход в систему",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(350, 20),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };

            Label lblLogin = new Label() { Text = "Логин:", Location = new Point(280, 80), AutoSize = true };
            txtLogin = new TextBox() { Location = new Point(370, 77), Width = 200 };

            Label lblPassword = new Label() { Text = "Пароль:", Location = new Point(280, 120), AutoSize = true };
            txtPassword = new TextBox() { Location = new Point(370, 117), Width = 200, PasswordChar = '*' };

            btnLogin = new Button()
            {
                Text = "ВОЙТИ",
                Location = new Point(370, 160),
                Width = 200,
                Height = 40,
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label lblCaptchaTitle = new Label()
            {
                Text = "СОБЕРИТЕ ПАЗЛ ПРАВИЛЬНО",
                Location = new Point(50, 230),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            lblCaptchaStatus = new Label()
            {
                Text = "ПАЗЛ НЕ СОБРАН",
                Location = new Point(450, 260),
                AutoSize = true,
                ForeColor = Color.Red,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnResetCaptcha = new Button()
            {
                Text = "СБРОСИТЬ ПАЗЛ",
                Location = new Point(350, 520),
                Width = 200,
                Height = 35,
                BackColor = Color.LightGray,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnResetCaptcha.Click += (s, e) => ResetCaptcha();

            string[] files = { "1.png", "2.png", "3.png", "4.png" };
            string captchaPath = Path.Combine(Application.StartupPath, "CaptchaImages");

            // Создаём папку если нет
            if (!Directory.Exists(captchaPath))
            {
                Directory.CreateDirectory(captchaPath);
                // Создаём тестовые картинки если их нет
                CreateTestImages(captchaPath);
            }

            Point[] piecePos = { new Point(50, 300), new Point(150, 300), new Point(50, 400), new Point(150, 400) };
            Point[] targetPos = { new Point(500, 300), new Point(600, 300), new Point(500, 400), new Point(600, 400) };

            for (int i = 0; i < 4; i++)
            {
                targetContents[i] = -1;
                string fullPath = Path.Combine(captchaPath, files[i]);

                pieces[i] = new PictureBox()
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = piecePos[i],
                    Size = new Size(80, 80),
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = i,
                    AllowDrop = true
                };

                if (File.Exists(fullPath))
                    pieces[i].Image = Image.FromFile(fullPath);
                else
                    pieces[i].BackColor = Color.Gray;

                int captureIndex = i;
                pieces[i].MouseDown += (s, e) => pieces[captureIndex].DoDragDrop(pieces[captureIndex], DragDropEffects.Move);
                pieces[i].DragEnter += (s, e) => e.Effect = DragDropEffects.Move;

                targets[i] = new PictureBox()
                {
                    BackColor = Color.LightGray,
                    Location = targetPos[i],
                    Size = new Size(80, 80),
                    BorderStyle = BorderStyle.FixedSingle,
                    AllowDrop = true,
                    Tag = i
                };

                int targetIndex = i;
                targets[i].DragEnter += (s, e) => e.Effect = DragDropEffects.Move;
                targets[i].DragDrop += (s, e) =>
                {
                    PictureBox dragged = (PictureBox)e.Data.GetData(typeof(PictureBox));
                    int draggedIndex = (int)dragged.Tag;

                    if (dragged != null && targetContents[targetIndex] == -1)
                    {
                        targets[targetIndex].Image = pieces[draggedIndex].Image;
                        targets[targetIndex].SizeMode = PictureBoxSizeMode.StretchImage;
                        pieces[draggedIndex].Visible = false;
                        targetContents[targetIndex] = draggedIndex;
                        CheckCaptchaComplete();
                    }
                };

                pnlAuth.Controls.Add(pieces[i]);
                pnlAuth.Controls.Add(targets[i]);
            }

            pnlAuth.Controls.Add(lblTitle);
            pnlAuth.Controls.Add(lblLogin);
            pnlAuth.Controls.Add(txtLogin);
            pnlAuth.Controls.Add(lblPassword);
            pnlAuth.Controls.Add(txtPassword);
            pnlAuth.Controls.Add(btnLogin);
            pnlAuth.Controls.Add(lblCaptchaTitle);
            pnlAuth.Controls.Add(lblCaptchaStatus);
            pnlAuth.Controls.Add(btnResetCaptcha);

            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(pnlAuth);
        }

        private void CreateTestImages(string path)
        {
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Gold };
            for (int i = 0; i < 4; i++)
            {
                Bitmap bmp = new Bitmap(80, 80);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(colors[i]);
                    g.DrawString((i + 1).ToString(), new Font("Arial", 30, FontStyle.Bold), Brushes.White, 25, 20);
                }
                bmp.Save(Path.Combine(path, $"{i + 1}.png"));
                bmp.Dispose();
            }
        }

        private void CheckCaptchaComplete()
        {
            bool allFilled = targetContents.All(t => t != -1);

            if (allFilled)
            {
                bool isCorrect = true;
                for (int i = 0; i < 4; i++)
                {
                    if (targetContents[i] != i)
                    {
                        isCorrect = false;
                        break;
                    }
                }

                if (isCorrect)
                {
                    lblCaptchaStatus.Text = "ПАЗЛ СОБРАН ПРАВИЛЬНО";
                    lblCaptchaStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblCaptchaStatus.Text = "ПАЗЛ СОБРАН НЕПРАВИЛЬНО";
                    lblCaptchaStatus.ForeColor = Color.Red;
                }
            }
            else
            {
                lblCaptchaStatus.Text = "ПАЗЛ НЕ СОБРАН";
                lblCaptchaStatus.ForeColor = Color.Red;
            }
        }

        private void ResetCaptcha()
        {
            for (int i = 0; i < 4; i++)
            {
                pieces[i].Visible = true;
                targets[i].Image = null;
                targets[i].BackColor = Color.LightGray;
                targetContents[i] = -1;
            }
            lblCaptchaStatus.Text = "ПАЗЛ НЕ СОБРАН";
            lblCaptchaStatus.ForeColor = Color.Red;
        }

        private bool IsCaptchaCorrect()
        {
            bool allFilled = targetContents.All(t => t != -1);
            if (!allFilled) return false;

            for (int i = 0; i < 4; i++)
            {
                if (targetContents[i] != i) return false;
            }
            return true;
        }

        private bool IsValidLogin(string login)
        {
            if (string.IsNullOrEmpty(login)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-zA-Z0-9а-яА-ЯёЁ._-]+$");
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                consecutiveFailures++;
                MessageBox.Show($"Введите логин и пароль. Попытка {consecutiveFailures} из 3", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (consecutiveFailures >= 3) ResetForm();
                return;
            }

            if (!IsValidLogin(login))
            {
                MessageBox.Show("Логин может содержать только буквы, цифры, точки, дефисы и подчеркивания", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existingUser = users.FirstOrDefault(u => u.Login == login);
            if (existingUser != null && existingUser.IsBlocked)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsCaptchaCorrect())
            {
                consecutiveFailures++;
                int remaining = 3 - consecutiveFailures;
                MessageBox.Show($"Пазл собран НЕПРАВИЛЬНО. Осталось попыток: {remaining}", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (consecutiveFailures >= 3 && existingUser != null)
                {
                    existingUser.IsBlocked = true;
                    MessageBox.Show($"Пользователь {login} заблокирован", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetForm();
                }
                else if (consecutiveFailures >= 3)
                {
                    ResetForm();
                }
                return;
            }

            var user = users.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                consecutiveFailures++;
                int remaining = 3 - consecutiveFailures;
                MessageBox.Show($"Неверный логин или пароль. Осталось попыток: {remaining}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (consecutiveFailures >= 3 && existingUser != null)
                {
                    existingUser.IsBlocked = true;
                    MessageBox.Show($"Пользователь {login} заблокирован", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetForm();
                }
                return;
            }

            consecutiveFailures = 0;
            MessageBox.Show($"Добро пожаловать, {login}!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            currentUser = login;
            currentRole = user.Role;

            if (user.Role == "Admin")
                ShowAdminPanel();
            else
                ShowUserPanel();
        }

        private void ResetForm()
        {
            txtLogin.Clear();
            txtPassword.Clear();
            ResetCaptcha();
            consecutiveFailures = 0;
        }

        private void InitializeUserPanel()
        {
            pnlUser = new Panel() { Dock = DockStyle.Fill };

            Label lblTitle = new Label()
            {
                Text = "ООО Два сеньора - Панель пользователя",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true
            };

            lblWelcome = new Label()
            {
                Text = "",
                Font = new Font("Arial", 12),
                Location = new Point(30, 60),
                AutoSize = true,
                ForeColor = Color.DarkGreen
            };

            btnUserLogout = new Button()
            {
                Text = "ВЫЙТИ",
                Location = new Point(30, 110),
                Width = 150,
                Height = 40,
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label lblProducts = new Label() { Text = "Наша продукция:", Location = new Point(30, 170), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            DataGridView dgvUserProducts = new DataGridView()
            {
                Location = new Point(30, 200),
                Size = new Size(500, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var productList = products.Select(p => new { Наименование = p.Name, Цена = $"{p.Price:C2}", Ед = p.Unit }).ToList();
            dgvUserProducts.DataSource = productList;

            Label lblOrders = new Label() { Text = "Последние заказы:", Location = new Point(560, 170), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            DataGridView dgvUserOrders = new DataGridView()
            {
                Location = new Point(560, 200),
                Size = new Size(580, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var orderList = orders.Select(o => new { Номер = o.Number, Дата = o.Date.ToShortDateString(), Заказчик = o.CustomerName, Сумма = $"{o.Total:C2}" }).ToList();
            dgvUserOrders.DataSource = orderList;

            pnlUser.Controls.Add(lblTitle);
            pnlUser.Controls.Add(lblWelcome);
            pnlUser.Controls.Add(btnUserLogout);
            pnlUser.Controls.Add(lblProducts);
            pnlUser.Controls.Add(dgvUserProducts);
            pnlUser.Controls.Add(lblOrders);
            pnlUser.Controls.Add(dgvUserOrders);

            btnUserLogout.Click += (s, e) => Logout();
            this.Controls.Add(pnlUser);
        }

        private void ShowUserPanel()
        {
            pnlAuth.Visible = false;
            pnlAdmin.Visible = false;
            pnlUser.Visible = true;
            lblWelcome.Text = $"Добро пожаловать, {currentUser}!";
        }

        private void InitializeAdminPanel()
        {
            pnlAdmin = new Panel() { Dock = DockStyle.Fill, AutoScroll = true };

            Label lblTitle = new Label()
            {
                Text = "ООО Два сеньора - Панель администратора",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            btnAdminLogout = new Button()
            {
                Text = "ВЫЙТИ",
                Location = new Point(1000, 20),
                Width = 120,
                Height = 35,
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            tabControl = new TabControl() { Location = new Point(20, 70), Size = new Size(1100, 650) };

            TabPage tpProducts = new TabPage("Продукция");
            InitProductsTab(tpProducts);
            tabControl.TabPages.Add(tpProducts);

            TabPage tpOrders = new TabPage("Заказы");
            InitOrdersTab(tpOrders);
            tabControl.TabPages.Add(tpOrders);

            TabPage tpProduction = new TabPage("Производство");
            InitProductionTab(tpProduction);
            tabControl.TabPages.Add(tpProduction);

            TabPage tpUsers = new TabPage("Пользователи");
            InitUsersTab(tpUsers);
            tabControl.TabPages.Add(tpUsers);

            pnlAdmin.Controls.Add(lblTitle);
            pnlAdmin.Controls.Add(btnAdminLogout);
            pnlAdmin.Controls.Add(tabControl);

            btnAdminLogout.Click += (s, e) => Logout();
            this.Controls.Add(pnlAdmin);
        }

        private void InitProductsTab(TabPage tab)
        {
            dgvProducts = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(700, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            RefreshProductsGrid();

            Label lblSearch = new Label() { Text = "Поиск:", Location = new Point(730, 15), AutoSize = true };
            txtSearchProduct = new TextBox() { Location = new Point(780, 12), Width = 150 };
            btnSearchProduct = new Button() { Text = "НАЙТИ", Location = new Point(940, 10), Width = 80 };
            btnSearchProduct.Click += (s, e) =>
            {
                string search = txtSearchProduct.Text.ToLower();
                var filtered = products.Where(p => p.Name.ToLower().Contains(search))
                    .Select(p => new { Наименование = p.Name, Цена = $"{p.Price:C2}", Себестоимость = $"{p.CostPrice:C2}", Маржа = $"{p.Price - p.CostPrice:C2}", Ед = p.Unit }).ToList();
                dgvProducts.DataSource = filtered;
            };

            btnExportReport = new Button()
            {
                Text = "ЭКСПОРТ ОТЧЕТА",
                Location = new Point(730, 50),
                Width = 290,
                Height = 35,
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnExportReport.Click += BtnExportReport_Click;

            tab.Controls.Add(dgvProducts);
            tab.Controls.Add(lblSearch);
            tab.Controls.Add(txtSearchProduct);
            tab.Controls.Add(btnSearchProduct);
            tab.Controls.Add(btnExportReport);
        }

        private void RefreshProductsGrid()
        {
            var data = products.Select(p => new
            {
                Наименование = p.Name,
                Цена = $"{p.Price:C2}",
                Себестоимость = $"{p.CostPrice:C2}",
                Маржа = $"{p.Price - p.CostPrice:C2}",
                Ед = p.Unit
            }).ToList();
            dgvProducts.DataSource = data;
        }

        private void InitOrdersTab(TabPage tab)
        {
            dgvOrders = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(700, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            RefreshOrdersGrid();

            Label lblDetails = new Label() { Text = "Детали заказа:", Location = new Point(10, 280), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            DataGridView dgvOrderDetails = new DataGridView()
            {
                Location = new Point(10, 310),
                Size = new Size(700, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvOrders.SelectionChanged += (s, e) =>
            {
                if (dgvOrders.SelectedRows.Count > 0)
                {
                    int orderNum = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["Номер"].Value);
                    var order = orders.FirstOrDefault(o => o.Number == orderNum);
                    if (order != null)
                    {
                        var details = order.Items.Select(i => new
                        {
                            Продукция = i.ProductName,
                            Кол_во = i.Quantity,
                            Ед = i.Unit,
                            Цена = $"{i.Price:C2}",
                            Сумма = $"{i.Sum:C2}"
                        }).ToList();
                        dgvOrderDetails.DataSource = details;
                    }
                }
            };

            Label lblSearchOrder = new Label() { Text = "Поиск заказа:", Location = new Point(730, 15), AutoSize = true };
            txtSearchOrder = new TextBox() { Location = new Point(830, 12), Width = 150 };
            btnSearchOrder = new Button() { Text = "НАЙТИ", Location = new Point(990, 10), Width = 80 };

            btnSearchOrder.Click += (s, e) =>
            {
                string search = txtSearchOrder.Text.ToLower();
                var filtered = orders.Where(o => o.CustomerName.ToLower().Contains(search) || o.Number.ToString().Contains(search))
                    .Select(o => new { Номер = o.Number, Дата = o.Date.ToShortDateString(), Заказчик = o.CustomerName, Сумма = $"{o.Total:C2}" }).ToList();
                dgvOrders.DataSource = filtered;
            };

            tab.Controls.Add(dgvOrders);
            tab.Controls.Add(lblDetails);
            tab.Controls.Add(dgvOrderDetails);
            tab.Controls.Add(lblSearchOrder);
            tab.Controls.Add(txtSearchOrder);
            tab.Controls.Add(btnSearchOrder);
        }

        private void RefreshOrdersGrid()
        {
            var data = orders.Select(o => new
            {
                Номер = o.Number,
                Дата = o.Date.ToShortDateString(),
                Заказчик = o.CustomerName,
                Исполнитель = o.Executor,
                Сумма = $"{o.Total:C2}"
            }).ToList();
            dgvOrders.DataSource = data;
        }

        private void InitProductionTab(TabPage tab)
        {
            dgvProduction = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(800, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            RefreshProductionGrid();

            Label lblMaterials = new Label() { Text = "Расход материалов:", Location = new Point(10, 280), Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            DataGridView dgvMaterials = new DataGridView()
            {
                Location = new Point(10, 310),
                Size = new Size(800, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvProduction.SelectionChanged += (s, e) =>
            {
                if (dgvProduction.SelectedRows.Count > 0)
                {
                    int prodNum = Convert.ToInt32(dgvProduction.SelectedRows[0].Cells["Номер"].Value);
                    var prod = productions.FirstOrDefault(p => p.Number == prodNum);
                    if (prod != null)
                    {
                        var materialsData = prod.Materials.Select(m => new
                        {
                            Материал = m.Name,
                            Количество = m.Quantity,
                            Ед = m.Unit
                        }).ToList();
                        dgvMaterials.DataSource = materialsData;
                    }
                }
            };

            tab.Controls.Add(dgvProduction);
            tab.Controls.Add(lblMaterials);
            tab.Controls.Add(dgvMaterials);
        }

        private void RefreshProductionGrid()
        {
            var data = productions.Select(p => new
            {
                Номер = p.Number,
                Дата = p.Date.ToShortDateString(),
                Продукция = p.ProductName,
                Количество = p.Quantity,
                Ед = p.Unit
            }).ToList();
            dgvProduction.DataSource = data;
        }

        private void InitUsersTab(TabPage tab)
        {
            dgvUsers = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(500, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            RefreshUsersGrid();

            Label lblNewLogin = new Label() { Text = "Логин:", Location = new Point(530, 15), AutoSize = true };
            TextBox txtNewLogin = new TextBox() { Location = new Point(600, 12), Width = 150 };

            Label lblNewPass = new Label() { Text = "Пароль:", Location = new Point(530, 45), AutoSize = true };
            TextBox txtNewPass = new TextBox() { Location = new Point(600, 42), Width = 150 };

            Label lblNewName = new Label() { Text = "ФИО:", Location = new Point(530, 75), AutoSize = true };
            TextBox txtNewName = new TextBox() { Location = new Point(600, 72), Width = 150 };

            Label lblNewRole = new Label() { Text = "Роль:", Location = new Point(530, 105), AutoSize = true };
            ComboBox cmbNewRole = new ComboBox() { Location = new Point(600, 102), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbNewRole.Items.AddRange(new string[] { "User", "Admin" });
            cmbNewRole.SelectedIndex = 0;

            Button btnAddUser = new Button() { Text = "ДОБАВИТЬ", Location = new Point(600, 135), Width = 150, Height = 35, BackColor = Color.LightGreen };
            Button btnUnblock = new Button() { Text = "РАЗБЛОКИРОВАТЬ", Location = new Point(10, 330), Width = 150, Height = 35, BackColor = Color.Gold };
            Button btnRefresh = new Button() { Text = "ОБНОВИТЬ", Location = new Point(170, 330), Width = 150, Height = 35, BackColor = Color.LightBlue };

            btnAddUser.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtNewLogin.Text) || string.IsNullOrEmpty(txtNewPass.Text))
                {
                    MessageBox.Show("Заполните логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (users.Any(u => u.Login == txtNewLogin.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                users.Add(new User
                {
                    Login = txtNewLogin.Text,
                    Password = txtNewPass.Text,
                    Role = cmbNewRole.SelectedItem.ToString(),
                    FullName = txtNewName.Text,
                    IsBlocked = false
                });

                RefreshUsersGrid();
                txtNewLogin.Clear();
                txtNewPass.Clear();
                txtNewName.Clear();
                MessageBox.Show("Пользователь добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnUnblock.Click += (s, e) =>
            {
                if (dgvUsers.SelectedRows.Count > 0)
                {
                    string login = dgvUsers.SelectedRows[0].Cells["Логин"].Value.ToString();
                    var user = users.FirstOrDefault(u => u.Login == login);
                    if (user != null)
                    {
                        user.IsBlocked = false;
                        RefreshUsersGrid();
                        MessageBox.Show($"Пользователь {login} разблокирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnRefresh.Click += (s, e) => RefreshUsersGrid();

            tab.Controls.Add(dgvUsers);
            tab.Controls.Add(lblNewLogin);
            tab.Controls.Add(txtNewLogin);
            tab.Controls.Add(lblNewPass);
            tab.Controls.Add(txtNewPass);
            tab.Controls.Add(lblNewName);
            tab.Controls.Add(txtNewName);
            tab.Controls.Add(lblNewRole);
            tab.Controls.Add(cmbNewRole);
            tab.Controls.Add(btnAddUser);
            tab.Controls.Add(btnUnblock);
            tab.Controls.Add(btnRefresh);
        }

        private void RefreshUsersGrid()
        {
            var data = users.Select(u => new
            {
                Логин = u.Login,
                ФИО = u.FullName,
                Роль = u.Role,
                Заблокирован = u.IsBlocked ? "Да" : "Нет"
            }).ToList();
            dgvUsers.DataSource = data;
        }

        private void ShowAdminPanel()
        {
            pnlAuth.Visible = false;
            pnlUser.Visible = false;
            pnlAdmin.Visible = true;
            RefreshProductsGrid();
            RefreshOrdersGrid();
            RefreshProductionGrid();
            RefreshUsersGrid();
        }

        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                Title = "Сохранить отчет",
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"Отчет_пиццерия_{DateTime.Now:yyyyMMdd_HHmmss}",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════════════");
                sb.AppendLine("              ОТЧЕТ ПО ДЕЯТЕЛЬНОСТИ ПИЦЦЕРИИ \"ДВА СЕНЬОРА\"");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine();

                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("1. ПРОДУКЦИЯ");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var p in products)
                {
                    sb.AppendLine($"   {p.Name}");
                    sb.AppendLine($"      Цена продажи: {p.Price:C2}");
                    sb.AppendLine($"      Себестоимость: {p.CostPrice:C2}");
                    sb.AppendLine($"      Маржа: {(p.Price - p.CostPrice):C2}");
                    sb.AppendLine();
                }

                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("2. ЗАКАЗЫ ПОКУПАТЕЛЕЙ");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var o in orders)
                {
                    sb.AppendLine($"   Заказ №{o.Number} от {o.Date:dd.MM.yyyy}");
                    sb.AppendLine($"   Заказчик: {o.CustomerName}");
                    sb.AppendLine($"   Исполнитель: {o.Executor}");
                    sb.AppendLine($"   Состав заказа:");
                    foreach (var i in o.Items)
                    {
                        sb.AppendLine($"      - {i.ProductName}: {i.Quantity} {i.Unit} x {i.Price:C2} = {i.Sum:C2}");
                    }
                    sb.AppendLine($"   ИТОГО: {o.Total:C2}");
                    sb.AppendLine();
                }

                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("3. ПРОИЗВОДСТВО");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var prod in productions)
                {
                    sb.AppendLine($"   Производство №{prod.Number} от {prod.Date:dd.MM.yyyy}");
                    sb.AppendLine($"   Продукция: {prod.ProductName} - {prod.Quantity} {prod.Unit}");
                    sb.AppendLine($"   Расход материалов:");
                    foreach (var m in prod.Materials)
                    {
                        sb.AppendLine($"      - {m.Name}: {m.Quantity} {m.Unit}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("═══════════════════════════════════════════════════════════════════════");
                sb.AppendLine("Конец отчета");

                File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Отчет сохранен:\n{saveDialog.FileName}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Logout()
        {
            currentUser = "";
            currentRole = "";
            ResetForm();
            ShowAuthPanel();
        }

        private void ShowAuthPanel()
        {
            pnlAuth.Visible = true;
            pnlAdmin.Visible = false;
            pnlUser.Visible = false;
            txtLogin.Clear();
            txtPassword.Clear();
            ResetCaptcha();
            consecutiveFailures = 0;
        }
    }

    // ==================== МОДЕЛИ ====================
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public string FullName { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public decimal CostPrice { get; set; }
    }

    public class OrderItem
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
    }

    public class Order
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public string Executor { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductionMaterial
    {
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }

    public class Production
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public List<ProductionMaterial> Materials { get; set; }
    }
}