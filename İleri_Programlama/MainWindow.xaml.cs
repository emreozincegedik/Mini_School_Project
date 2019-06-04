using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace İleri_Programlama
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string cmdStr = "Select department_name from tbl_department";

            List<dbConnection.cmdParameterType> lstParamsQuickerWay = new List<dbConnection.cmdParameterType> { };

            var dtTable = dbConnection.DB_Select(cmdStr, lstParamsQuickerWay);

            if (dtTable!=null)
            {
                foreach (DataRow row in dtTable.Rows)
                {
                    register_department.Items.Add(row["department_name"].ToString());
                }
            }
        }

        private void Register(object sender, RoutedEventArgs e)
        {
            if (register_name.Text.Length==0)
            {
                MessageBox.Show("Name is empty.");
                return;
            }
            if (register_surname.Text.Length==0)
            {
                MessageBox.Show("Surname is empty.");
                return;
            }
            if (register_email.Text.Length==0)
            {
                MessageBox.Show("E-mail is empty.");
                return;
            }
            if (register_password.Password.Length==0)
            {
                MessageBox.Show("Password is empty.");
                return;
            }
            if (register_phone.Text.Length==0)
            {
                MessageBox.Show("Phone is empty.");
                return;
            }
            if (!register_name.Text.All(Char.IsLetter))
            {
                MessageBox.Show("Name can only contain letters.");
                return;
            }
            if (!register_surname.Text.All(Char.IsLetter))
            {
                MessageBox.Show("Surname can only contain letters.");
                return;
            }
            if (!register_phone.Text.All(Char.IsNumber))
            {
                MessageBox.Show("Phone can only contain numbers.");
                return;
            }
            
            if (register_department.SelectedIndex==-1)
            {
                MessageBox.Show("You must choose a department.");
                return;
            }



            DataTable dtTable = new DataTable();


            string cmdStr = "select * from tbl_users where email=@email or phone=@phone";

            List<dbConnection.cmdParameterType> lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@email", register_email.Text.Trim().ToLower()),
                new dbConnection.cmdParameterType("@Phone", register_phone.Text) };

            dtTable=dbConnection.DB_Select(cmdStr, lstParamsQuickerWay);
            if (dtTable.Rows.Count > 0)
            {
                MessageBox.Show("Email or phone is already registered!");
                return;
            }

            string role = "3";
            if (register_role_teacher.IsChecked==true)
            {
                role = "passive";
            }
            cmdStr =
                "Insert into tbl_users(name,surname,password,email,phone,role,department) values (@name,@surname,@pass,@email,@phone,@role,@department)";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@name", register_name.Text.Trim()),
                new dbConnection.cmdParameterType("@surname", register_surname.Text.Trim()),
                new dbConnection.cmdParameterType("@pass", register_password.Password),
                new dbConnection.cmdParameterType("@email", register_email.Text.Trim().ToLower()),
                new dbConnection.cmdParameterType("@phone", register_phone.Text.Trim()),
                new dbConnection.cmdParameterType("@role", role),
                new dbConnection.cmdParameterType("@department", register_department.SelectedItem),
            };
            dtTable=dbConnection.DB_Select(cmdStr, lstParamsQuickerWay);

            if (role=="3")
            {
                cmdStr = $"INSERT INTO tbl_students(email,grade,registration_year) VALUES (@email,1,{DateTime.Now.Year})";
                lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                    new dbConnection.cmdParameterType("@email", register_email.Text.Trim().ToLower())};
                dtTable=dbConnection.DB_Select(cmdStr, lstParamsQuickerWay);
            }

            string teacher = "";
            if (register_role_teacher.IsChecked == true)
            {
                teacher = "Please wait for admin to approve your registration.";
            }
            MessageBox.Show("Registration Successful. "+teacher);
            tab_login.Focus();
            register_name.Text = register_surname.Text =
                register_email.Text = register_phone.Text = register_password.Password = "";


        }

        private void Login(object sender, RoutedEventArgs e)
        {
            if (login_email.Text.Length==0)
            {
                MessageBox.Show("E-mail is empty.");
                return;
            }
            if (login_password.Password.Length == 0)
            {
                MessageBox.Show("Password is empty.");
                return;
            }
            

            string cmdstr = "select * from tbl_users where email=@email";

            List<dbConnection.cmdParameterType> lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@email", login_email.Text.Trim().ToLower()) };

            var dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            if (dtTable.Rows.Count == 0)
            {
                MessageBox.Show("E-mail isn't registered in database");
                return;
            }
            if (dtTable.Rows.Count >1)
            {
                MessageBox.Show("Database has more than 1 entries with given e-mail please contact admin.");
                return;
            }

            cmdstr = "select * from tbl_users where email=@email and password=@Password";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@email", login_email.Text.Trim().ToLower()),
                new dbConnection.cmdParameterType("@Password", login_password.Password) };

            dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            if (dtTable.Rows.Count == 0)
            {
                MessageBox.Show("Wrong Password.");
                return;
            }


            if (dtTable.Rows.Count ==1)
            {
                string role = dtTable.Rows[0]["role"].ToString();
                if (role == "denied")
                {
                    MessageBox.Show("You have been denied for teacher registration. ");
                    return;
                }
                if (role == "passive")
                {
                    MessageBox.Show("Please wait for admin to approve your registration. ");
                    return;
                }
                MessageBox.Show("Welcome. ");
                mem_info(dtTable);
                
                if (member_role.Content.ToString() == "Admin")
                {
                    admin(dtTable);
                    tab_admin.IsEnabled = true;
                    tab_admin.Focus();
                }
                else if (member_role.Content.ToString() == "Teacher")
                {
                    teacher(dtTable);
                    tab_teacher.IsEnabled = true;
                    tab_teacher.Focus();
                }
                else if (member_role.Content.ToString() == "Student")
                {
                    student(dtTable);
                    tab_student.IsEnabled = true;
                    tab_student.Focus();
                }
                else
                {
                    MessageBox.Show("User is incorrectly defined in database. Please contact admin.");
                    return;
                }

                mem_info(dtTable);
                tab_login.IsEnabled = tab_register.IsEnabled = false;
                tab_membership.IsEnabled = true;

                login_password.Password = "";
            }
            else
            {
                MessageBox.Show("Unknown error. Please contact admin."); //I don't know how this would trigger but who knows.
                return;
            }
        }

        private void change_Email(object sender, RoutedEventArgs e)
        {
            if (member_email_new.Text.Trim().Length==0)
            {
                MessageBox.Show("E-mail is empty.");
                return;
            }

            string cmdstr = "Select * from tbl_users where email=@email";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@email", member_email_new.Text.Trim().ToLower()) };

            var dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            

            if (dtTable.Rows.Count > 0)
            {
                MessageBox.Show("E-mail is already registered. Please try another email!");
                return;
            }

            cmdstr = "Update tbl_users Set email=@newemail where email=@email";

            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
               new dbConnection.cmdParameterType("@email", member_email.Content.ToString().Trim().ToLower()),
               new dbConnection.cmdParameterType("@newemail",member_email_new.Text.Trim().ToLower())
            };
            dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            
            if (member_role.Content.ToString()== "Student")
            {
                cmdstr = "Update tbl_lesson_notes Set lesson_student_email=@newemail where lesson_student_email=@email";
                dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                cmdstr = "Update tbl_students Set email=@newemail where email=@email";
                dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            }
            else if (member_role.Content.ToString() == "Teacher")
            {
                cmdstr = "Update tbl_lesson_notes Set lesson_teacher_email=@newemail where lesson_teacher_email=@email";
                dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                cmdstr = "Update tbl_lessons Set lesson_teacher_email=@newemail where lesson_teacher_email=@email";
                dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            }
            

             member_email.Content = member_email_new.Text.ToLower().Trim();
             MessageBox.Show("Successfully changed email!");
        }

        private void change_Password(object sender, RoutedEventArgs e)
        {
            if (member_password_new.Password.Length==0)
            {
                MessageBox.Show("Password is empty.");
                return;
            }
            var cmdstr = "Update tbl_users Set password=@newpassword where email=@email";

            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@newpassword", member_password_new.Password),  
                new dbConnection.cmdParameterType("@email", member_email.Content.ToString().Trim().ToLower())
            };

            var dtTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            MessageBox.Show("Successfully changed password!");
        }

        void admin(DataTable user_table)
        {
            admin_greeting.Content = user_table.Rows[0]["name"].ToString();

            string cmdstr = "Select name, surname, email, phone, role, department from tbl_users where role!='passive' and role!='denied'";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>();
            var everyone = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            admin_registered_users.DataContext = everyone.AsDataView();

            cmdstr = "Select department_name from tbl_department";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>();
            var departments = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            for (int i = 0; i < departments.Rows.Count; i++)
            {
                admin_department_combobox.Items.Add(departments.Rows[i]["department_name"].ToString());
            }

            cmdstr = "Select lesson_name, lesson_teacher_name,lesson_teacher_email, lesson_department from tbl_lessons where active=0";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>();
            var lessons_waiting_for_approval = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            admin_lessons_awaiting.DataContext = lessons_waiting_for_approval.AsDataView();

            cmdstr = "Select lesson_name, lesson_teacher_name,lesson_teacher_email, lesson_department from tbl_lessons where active=1";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>();
            var lessons_approved= dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            admin_lesson_list.DataContext = lessons_approved.AsDataView();

            cmdstr = "Select name, surname, email, phone, role, department from tbl_users where role='passive' ";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>();
            var teachers_waiting = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            admin_teacher_awaiting.DataContext = teachers_waiting.AsDataView();


        }

        void teacher(DataTable user_table)
        {
            teacher_greeting.Content = user_table.Rows[0]["name"].ToString();
            var cmdstr = "Select lesson_name from tbl_lessons where lesson_teacher_email=@email and active=1";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@email", user_table.Rows[0]["email"])
            };
            var teacher_classes_active = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            teacher_lessons_approved.DataContext = teacher_classes_active.AsDataView();

            cmdstr = "Select lesson_name from tbl_lessons where lesson_teacher_email=@email and active=0";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@email", user_table.Rows[0]["email"])
            };
            var teacher_classes_passive = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            teacher_lessons_pending.DataContext = teacher_classes_passive.AsDataView();

            cmdstr = "Select lesson_name, lesson_student_email, lesson_student_name, lesson_student_surname, lesson_midterm, lesson_final from tbl_lesson_notes where lesson_teacher_email=@email and pending='0'";
            
            var teacher_classes_approved = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            teacher_students_list.DataContext = teacher_classes_approved.AsDataView();

            cmdstr = "Select lesson_name, lesson_student_email, lesson_student_name, lesson_student_surname from tbl_lesson_notes where lesson_teacher_email=@email and pending='1'";
            var teacher_classes_student_pending = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            teacher_students_awaiting.DataContext = teacher_classes_student_pending.AsDataView();

        }

        void student(DataTable user_table)
        {
            var cmdstr = "Select * from tbl_students where email=@email";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@email", user_table.Rows[0]["email"])
            };

            var student_info = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            student_grade.Content=DateTime.Now.Year -Convert.ToInt32(student_info.Rows[0]["registration_year"].ToString())+ Convert.ToInt32(student_info.Rows[0]["grade"].ToString());
            student_greeting.Content = user_table.Rows[0]["name"].ToString();



            cmdstr = "Select lesson_name,lesson_teacher_name, lesson_department, lesson_midterm,lesson_final,grade from tbl_lesson_notes where lesson_student_email=@email and pending=0";
            var studentTable = new DataTable();

            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> { new dbConnection.cmdParameterType("@email", user_table.Rows[0]["email"]), };
            studentTable = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            student_lessons_registered.DataContext = studentTable.AsDataView();


            
            int general_sum = 0;
            int classes = 0;
            int yearly_sum = 0;
            int classes2 = 0;
            for (int i = 0; i < studentTable.Rows.Count; i++)
            {
                if (studentTable.Rows[i]["lesson_midterm"].ToString()!="")
                {
                    general_sum += Convert.ToInt32(studentTable.Rows[i]["lesson_midterm"]);
                    classes++;
                    if (studentTable.Rows[i]["grade"].ToString() == student_grade.Content.ToString() )
                    {
                        yearly_sum += Convert.ToInt32(studentTable.Rows[i]["lesson_midterm"]);
                        classes2++;
                    }
                }

                if (studentTable.Rows[i]["lesson_final"].ToString()!="")
                {
                    general_sum += Convert.ToInt32(studentTable.Rows[i]["lesson_final"]);
                    classes++;
                    if (studentTable.Rows[i]["grade"].ToString() ==student_grade.Content.ToString())
                    {
                        yearly_sum += Convert.ToInt32(studentTable.Rows[i]["lesson_final"]);
                        classes2++;
                    }
                }
                
            }

            if (classes!=0)
            {
                general_sum /= classes;
            }

            if (classes2!=0)
            {
                yearly_sum /= classes2;
            }
            
            
            
           
            student_general_average.Content = general_sum.ToString();
            student_yearly_average.Content = yearly_sum.ToString();

            cmdstr = "Select lesson_name,lesson_teacher_name, lesson_department from tbl_lesson_notes where lesson_student_email=@email and pending=1";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> { new dbConnection.cmdParameterType("@email", user_table.Rows[0]["email"]), };
            var pending = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            student_lessons_pending.DataContext = pending.AsDataView();

            DataTable merge=new DataTable();
            merge.Merge(studentTable);
            merge.Merge(pending);

            cmdstr = "Select lesson_name, lesson_teacher_name, lesson_department from tbl_lessons where active=1";
            var remaining=new DataTable();
            remaining= dbConnection.DB_Select(cmdstr,new List<dbConnection.cmdParameterType>());
            for (int j = 0; j < merge.Rows.Count; j++)
            {
                for (int i = 0; i < remaining.Rows.Count; i++)
                {
                    if (remaining.Rows[i]["lesson_name"].ToString()==merge.Rows[j]["lesson_name"].ToString())
                    {
                        remaining.Rows[i].Delete();
                        remaining.AcceptChanges();
                        i--;
                    }
                }
            }
            student_lessons_available.DataContext = remaining.AsDataView();


        }




        private void goto_member(object sender, RoutedEventArgs e)
        {
            tab_membership.IsEnabled = true;
            tab_membership.Focus();
            tab_admin.IsEnabled= tab_student.IsEnabled = tab_admin.IsEnabled = tab_teacher.IsEnabled = false;
        }

        private void logout(object sender, RoutedEventArgs e)
        {
            tab_register.IsEnabled=tab_login.IsEnabled = true;
            tab_login.Focus();
            tab_membership.IsEnabled = tab_student.IsEnabled = tab_admin.IsEnabled = tab_teacher.IsEnabled = false;
            teacher_class_request_name.Text = teacher_midterm.Text= teacher_final.Text= admin_update_password.Text= admin_lesson_name_new.Text= admin_department_name.Text= "";
            admin_department_combobox.Items.Clear();
        }

        private void go_back(object sender, RoutedEventArgs e)
        {
            tab_membership.IsEnabled = tab_student.IsEnabled = tab_admin.IsEnabled = tab_teacher.IsEnabled = false;
            if (member_role.Content.ToString() == "Student")
            {
                tab_student.IsEnabled = true;
                tab_student.Focus();
            }
            else if (member_role.Content.ToString() == "Teacher")
            {
                tab_teacher.IsEnabled = true;
                tab_teacher.Focus();
            }
            else
            {
                tab_admin.IsEnabled = true;
                tab_admin.Focus();
            }
            tab_membership.IsEnabled = false;
        }

        private void Admin_department_apply_Click(object sender, RoutedEventArgs e)
        {
           
            if (admin_department_update.IsChecked==true)
            {
                if (admin_department_combobox.SelectedIndex==-1)
                {
                    MessageBox.Show("Please select a department before proceeding. ");
                    return;
                }
                if (!Regex.IsMatch(admin_department_name.Text.Trim(), @"^[\w ]+$"))
                {
                    MessageBox.Show("Department name can only contain letters.");
                    return;
                }
 
                for (int i = 0; i < admin_department_combobox.Items.Count; i++)
                {
                    if (admin_department_combobox.Items[i].ToString()== admin_department_name.Text.Trim().ToLower())
                    {
                        MessageBox.Show("Department already exists. ");
                        return;
                    }
                }
                 
                
                 
                string cmdstr = "Update tbl_department set department_name=@dnewname where department_name=@name";
                var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                   new dbConnection .cmdParameterType("@name", admin_department_combobox.Items[admin_department_combobox.SelectedIndex]),
                   new dbConnection. cmdParameterType("@dnewname", admin_department_name.Text.Trim().ToLower()),
                }; 
                var  update_department = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);


                cmdstr = "Update tbl_users set department=@dnewname where department=@name";
                var update_users_department = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                cmdstr= "Update tbl_lessons set lesson_department=@dnewname where lesson_department=@name";
                var update_lessons_department = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                cmdstr = "Update tbl_lesson_notes set lesson_department=@dnewname where lesson_department=@name";
                var update_lesson_notes_department = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                wait = true;
                int selected_index = admin_department_combobox.SelectedIndex;
                admin_department_combobox.Items[admin_department_combobox.SelectedIndex] = 
                    register_department.Items[selected_index] =
                    admin_department_name.Text.Trim().ToLower();

                
                admin_department_combobox.SelectedIndex = selected_index;
                wait = false;


            }
            else if (admin_department_create.IsChecked==true)
            {
                if (!Regex.IsMatch(admin_department_name.Text.Trim(), @"^[\w ]+$"))
                {
                   MessageBox.Show("Department name can only contain letters.");
                   return;
                }

                if (admin_department_name.Text.Trim().ToLower()=="admin")
                {
                    MessageBox.Show("Admin can't be department name");
                    return;
                }
                for (int i = 0; i < admin_department_combobox.Items.Count; i++)
                {
                    if (admin_department_combobox.Items[i].ToString() == admin_department_name.Text.Trim().ToLower())
                    {
                        MessageBox.Show("Department already exists. ");
                        return;
                    }
                }
                string cmdstr = "Insert into tbl_department(department_name) values (@name)";
                var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                    new dbConnection.cmdParameterType("@name", admin_department_name.Text.Trim().ToLower()),
                   
                };
                var add_department = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                admin_department_combobox.Items.Add(admin_department_name.Text.Trim().ToLower());
                register_department.Items.Add(admin_department_name.Text.Trim().ToLower());


            }
            else //delete
            {
                if (admin_department_combobox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a department before proceeding. ");
                    return;
                }

                if (MessageBox.Show("Are you sure? Everyone associated with the department will also be deleted. ", "This is irreversible!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string cmdstr = "Select email from tbl_users where department=@name and role='3'";
                    var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                        new dbConnection .cmdParameterType("@name", admin_department_combobox.Items[admin_department_combobox.SelectedIndex]),
                    };
                    var department_students = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                    for (int i = 0; i < department_students.Rows.Count; i++)
                    {
                        cmdstr = "Delete from tbl_students where email=@email";
                        var lstParamsQuickerWay2 = new List<dbConnection.cmdParameterType>
                        {
                            new dbConnection .cmdParameterType("@email",department_students.Rows[i]["email"].ToString()),
                        };
                        var department_students_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay2);
                    }
                    cmdstr = "Delete from tbl_department where department_name=@name";
                    var department__users_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    cmdstr = "Delete from tbl_department where department_name=@name";
                    
                    var department_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    cmdstr = "Delete from tbl_lesson_notes where lesson_department=@name";
                    var department_lesson_notes_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    cmdstr = "Delete from tbl_lessons where lesson_department=@name";
                    var department_lessons_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    admin_department_combobox.Items.Remove(admin_department_combobox.Items[admin_department_combobox.SelectedIndex]);
                    register_department.Items.Remove(admin_department_combobox.Items[admin_department_combobox.SelectedIndex]);
                }

            }
           MessageBox.Show("Done. ");
        }

        private void Admin_class_name_update_Click(object sender, RoutedEventArgs e)
        {
            if (admin_lesson_list.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please select only one row. ");
                return;
            }
            if (!Regex.IsMatch(admin_lesson_name_new.Text.Trim(), @"^[\w ]+$"))
            {
                MessageBox.Show("New lesson name can only contain letters and can't be empty.");
                return;
            }
            DataView view = (DataView)admin_lesson_list.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (admin_lesson_name_new.Text.Trim().ToLower()==table.Rows[i]["lesson_name"].ToString())
                {
                    MessageBox.Show("Can't change name. Class already exists and it is thought by " + table.Rows[i]["lesson_teacher_name"]);
                    return;
                }
            }

            string cmdstr = "Update tbl_lessons set lesson_name=@lnewname where lesson_name=@lname and lesson_teacher_email=@temail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@lnewname", admin_lesson_name_new.Text.Trim().ToLower()),
                new dbConnection.cmdParameterType("@lname", table.Rows[admin_lesson_list.SelectedIndex]["lesson_name"].ToString().Trim().ToLower()),
                new dbConnection.cmdParameterType("@temail",table.Rows[admin_lesson_list.SelectedIndex]["lesson_teacher_email"].ToString().Trim().ToLower()),
            };
            var change_class_name = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            cmdstr = "Update tbl_lesson_notes set lesson_name=@lnewname where lesson_name=@lname and lesson_teacher_email=@temail";
            var change_class_notes_name = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            cmdstr = "Delete from tbl_lessons where lesson_teacher_email!=@temail and lesson_name=@lname";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@lname", admin_lesson_name_new.Text.Trim()),
                new dbConnection.cmdParameterType("@temail",table.Rows[admin_lesson_list.SelectedIndex]["lesson_teacher_email"].ToString().Trim().ToLower()),
            };
            var delete_previous_pendings = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            table.Rows[admin_lesson_list.SelectedIndex]["lesson_name"] = admin_lesson_name_new.Text.Trim().ToLower();
            wait = true; //admin_lesson_list selection changed doesn't work
            admin_lesson_list.DataContext = table.AsDataView();
            wait = false;
            view = (DataView)admin_lessons_awaiting.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view);
            string extra = "";
            for (int i = 0; i < table2.Rows.Count; i++)
            {
                if (admin_lesson_name_new.Text.Trim().ToLower() == table2.Rows[i]["lesson_name"].ToString())
                {
                    extra += table2.Rows[i]["lesson_teacher_name"]+", ";
                    table2.Rows.Remove(table2.Rows[i]);
                    i--;
                }
            }

            if (extra.Length>0)
            {
                extra += "who demanded the class are also removed from pending";
            }
            admin_lessons_awaiting.DataContext = table2.AsDataView();
            MessageBox.Show("Success. "+extra);
        }

        private void Admin_password_update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (admin_registered_users.SelectedItems.Count!=1)
            {
                MessageBox.Show("Select only one row from registered users.");
                return;
            }
            if (admin_update_password.Password.Length==0)
            {
                MessageBox.Show("Password can't be empty.");
                return;
            }
            DataView view = (DataView)admin_registered_users.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            if (table.Rows[admin_registered_users.SelectedIndex]["role"].ToString()=="1")
            {
                MessageBox.Show("Can't change other admins' password. ");
                return;
            }
            var cmdstr= "Update tbl_users set password=@pass where email=@email";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@pass", admin_update_password.Password),
                new dbConnection.cmdParameterType("@email", table.Rows[admin_registered_users.SelectedIndex]["email"].ToString().ToLower().Trim())
            };
            var change_password = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            MessageBox.Show("Successfully changed selected user's password.");
        }

        private void Teacher_class_request_btn_Click(object sender, RoutedEventArgs e)
        {
            if (teacher_class_request_name.Text.Trim().Length==0)
            {
                MessageBox.Show("Class name can't be empty.");
                return;
            }
            if (!Regex.IsMatch(teacher_class_request_name.Text.Trim(), @"^[\w ]+$"))
            {
                MessageBox.Show("Class name can only contain letters.");
                return;
            }

            string cmdstr =
                "Select lesson_name, lesson_teacher_name, active from tbl_lessons where lesson_name=@lname and active='1'";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {new dbConnection.cmdParameterType("@lname", teacher_class_request_name.Text.ToLower().Trim())
            };
            var existing_classes= dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            if (existing_classes.Rows.Count>0)
            {
                MessageBox.Show("Class already exists and it is thought by " + existing_classes.Rows[0]["lesson_teacher_name"]);
                return;
            }
            DataView view = (DataView)teacher_lessons_pending.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i]["lesson_name"].ToString()== teacher_class_request_name.Text.ToLower().Trim())
                {
                    MessageBox.Show("You have already requested this lesson!");
                    return;
                }
            }
            view = (DataView)teacher_lessons_approved.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view);
            
            for (int i = 0; i < table2.Rows.Count; i++)
            {
                if (table2.Rows[i]["lesson_name"].ToString()== teacher_class_request_name.Text.ToLower().Trim())
                {
                    MessageBox.Show("You are already giving this lesson!");
                    return;
                }   
            }

            cmdstr =
                "Insert into tbl_lessons (lesson_name, lesson_teacher_name, lesson_teacher_email, lesson_department, active) values (@lname, @tname, @temail, @ldepartment, '0')";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@lname", teacher_class_request_name.Text.ToLower().Trim()),
                new dbConnection.cmdParameterType("@tname", member_name.Content.ToString().Trim()),
                new dbConnection.cmdParameterType("@temail", member_email.Content.ToString().Trim()),
                new dbConnection.cmdParameterType("@ldepartment", member_department.Content.ToString().Trim().ToLower())
            };
            var request_class= dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);



            MessageBox.Show("Requested class. Please wait for admin to approve your request.");


            
            table.Rows.Add(teacher_class_request_name.Text.ToLower().Trim());
            teacher_lessons_pending.ItemsSource = table.AsDataView();


        }

        private void Teacher_update_Click(object sender, RoutedEventArgs e)
        {
            if (teacher_students_list.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please select only one student!");
                return;
            }

            if (teacher_midterm.Text.Trim().Length==0 && teacher_final.Text.Trim().Length==0)
            {
                MessageBox.Show("Please fill at least one of the notes.");
                return;
            }
            if (!Regex.IsMatch(teacher_midterm.Text, @"^\d*$") || !Regex.IsMatch(teacher_final.Text, @"^\d*$")|| (teacher_midterm.Text != "" && teacher_final.Text != "") && (Int32.Parse(teacher_midterm.Text) < 0 || Int32.Parse(teacher_midterm.Text) > 100 || Int32.Parse(teacher_final.Text) < 0 || Int32.Parse(teacher_final.Text) > 100))
            {
                MessageBox.Show("Midterm and Final can only be a number between 0 and 100.");
                return;
            }
            DataView view = (DataView)teacher_students_list.ItemsSource;
            DataTable table = DataViewAsDataTable(view);


            string cmdstr = "Update tbl_lesson_notes set lesson_midterm=@mid, lesson_final=@final where lesson_name=@lname and lesson_student_email=@semail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> {
                new dbConnection.cmdParameterType("@lname", table.Rows[teacher_students_list.Items.IndexOf( teacher_students_list.SelectedItem)]["lesson_name"].ToString().Trim()),
                new dbConnection.cmdParameterType("@semail",table.Rows[teacher_students_list.Items.IndexOf( teacher_students_list.SelectedItem)]["lesson_student_email"].ToString().Trim().ToLower()),
                new dbConnection.cmdParameterType("@mid", teacher_midterm.Text),
                new dbConnection.cmdParameterType("@final", teacher_final.Text),
            };
            var note_update = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            table.Rows[teacher_students_list.Items.IndexOf(teacher_students_list.SelectedItem)]["lesson_midterm"] =
                teacher_midterm.Text;
            table.Rows[teacher_students_list.Items.IndexOf(teacher_students_list.SelectedItem)]["lesson_final"] =
                teacher_final.Text;
            wait = true;
            int selected_index = teacher_students_list.SelectedIndex;
            teacher_students_list.DataContext = table.AsDataView();
            teacher_students_list.SelectedIndex = selected_index;
            wait = false;
            MessageBox.Show("Success");
        }


        private void mem_info(DataTable dtTable)
        {
            member_email.Content = dtTable.Rows[0]["email"].ToString().Trim().ToLower();
            member_name.Content= dtTable.Rows[0]["name"].ToString();
            member_surname.Content = dtTable.Rows[0]["surname"].ToString();
            member_department.Content = dtTable.Rows[0]["department"].ToString().Trim().ToLower();
            if (member_department.Content.ToString()=="")
            {
                member_department.Content = "Admin";
            }
            member_phone.Content = dtTable.Rows[0]["phone"];
            member_role.Content = 
                dtTable.Rows[0]["role"].ToString() == "3" ? "Student" :
                dtTable.Rows[0]["role"].ToString() == "2" ? "Teacher" :
                                                            "Admin";
        }

        private void Student_lessons_pending_delete_Click(object sender, RoutedEventArgs e)
        {
            if (student_lessons_pending.SelectedIndex==-1)
            {
                MessageBox.Show("Please select a class to delete");
                return;
            }

            DataView view = (DataView)student_lessons_pending.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            DataView view2 = (DataView) student_lessons_available.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view2);
            for (int i = 0; i < student_lessons_pending.SelectedItems.Count; i++)
            {
                var cmdstr = "Delete from tbl_lesson_notes where lesson_student_email=@email and lesson_name=@lname";
                var lstParamsQuickerWay = new List<dbConnection.cmdParameterType> { new dbConnection.cmdParameterType("@lname", table.Rows[student_lessons_pending.Items.IndexOf(student_lessons_pending.SelectedItems[i])]["lesson_name"]),
                                                                                    new dbConnection.cmdParameterType("@email", member_email.Content) };
                var pending = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                table2.ImportRow(table.Rows[student_lessons_pending.Items.IndexOf(student_lessons_pending.SelectedItems[i])]);
                student_lessons_available.ItemsSource = table2.AsDataView();
                table.Rows.Remove(table.Rows[student_lessons_pending.Items.IndexOf(student_lessons_pending.SelectedItems[i])]);
                student_lessons_pending.ItemsSource= table.AsDataView();
            }

        }

        public static DataTable DataViewAsDataTable(DataView dv)
        {
            DataTable dt = dv.Table.Clone();
            foreach (DataRowView drv in dv)
                dt.ImportRow(drv.Row);
            return dt;
        }

        private void Student_lessons_apply_Click(object sender, RoutedEventArgs e)
        {
            if (student_lessons_available.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a class to delete");
                return;
            }

            DataView view2 = (DataView)student_lessons_pending.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view2);
            DataView view = (DataView)student_lessons_available.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            for (int i = 0; i < student_lessons_available.SelectedItems.Count; i++)
            {
                var cmdstr =
                    "Select lesson_id,lesson_name,lesson_teacher_name,lesson_teacher_email,lesson_department from tbl_lessons where lesson_name=@lname and lesson_teacher_name=@tname";
                var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
                {
                    new dbConnection.cmdParameterType("@lname",
                        table.Rows[student_lessons_available.Items.IndexOf(student_lessons_available.SelectedItems[i])][
                            "lesson_name"].ToString()),
                    new dbConnection.cmdParameterType("@tname",table.Rows[student_lessons_available.Items.IndexOf(student_lessons_available.SelectedItems[i])][
                        "lesson_teacher_name"].ToString())
                };
                var pending = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                //MessageBox.Show(pending.Rows[0]["lesson_id"].ToString());

                cmdstr =
                    "Insert into tbl_lesson_notes(lesson_id, lesson_name, lesson_student_email, lesson_student_name,lesson_student_surname,lesson_teacher_email,lesson_teacher_name,lesson_department,pending,grade) " +
                    $"values ({pending.Rows[0]["lesson_id"].ToString()},@lname,@semail,@sname,@ssurname,@temail,@tname,@department,1,{student_grade.Content})";
                lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
                {
                    new dbConnection.cmdParameterType("@lname",pending.Rows[0]["lesson_name"]),
                    new dbConnection.cmdParameterType("@semail",member_email.Content.ToString().Trim().ToLower()),
                    new dbConnection.cmdParameterType("@sname",member_name.Content),
                    new dbConnection.cmdParameterType("@ssurname",member_surname.Content),
                    new dbConnection.cmdParameterType("@tname",pending.Rows[0]["lesson_teacher_name"].ToString()),
                    new dbConnection.cmdParameterType("@temail",pending.Rows[0]["lesson_teacher_email"].ToString().Trim().ToLower()),
                    new dbConnection.cmdParameterType("@department",pending.Rows[0]["lesson_department"].ToString().Trim().ToLower()),
                };
                pending = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                table2.ImportRow(table.Rows[student_lessons_available.Items.IndexOf(student_lessons_available.SelectedItems[i])]);
                    student_lessons_pending.ItemsSource = table2.AsDataView();
                    table.Rows.Remove(table.Rows[student_lessons_available.Items.IndexOf(student_lessons_available.SelectedItems[i])]);
                    student_lessons_available.ItemsSource = table.AsDataView();

            }


        }

        private void Teacher_pending_delete_Click(object sender, RoutedEventArgs e)
        {
            if (teacher_lessons_pending.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please select only one class!");
                return;
            }
            DataView view = (DataView)teacher_lessons_pending.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
                
            var cmdstr =
                "Delete from tbl_lessons where lesson_name=@lname and lesson_teacher_email=@temail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@lname", table.Rows[teacher_lessons_pending.SelectedIndex]["lesson_name"].ToString().ToLower().Trim()),
                new dbConnection.cmdParameterType("@temail", member_email.Content)
            };
            var delete= dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            table.Rows.Remove(table.Rows[teacher_lessons_pending.SelectedIndex]);
            
            teacher_lessons_pending.ItemsSource= table.AsDataView();
            MessageBox.Show("Successfully deleted classes");
        }

        bool wait = false;
        private void teacher_select_note(object sender, SelectionChangedEventArgs e)
        {
            if (!wait)
            {
                DataView view = (DataView)teacher_students_list.ItemsSource;
                DataTable table = DataViewAsDataTable(view);
                teacher_midterm.Text = table.Rows[teacher_students_list.SelectedIndex]["lesson_midterm"].ToString();
                teacher_final.Text = table.Rows[teacher_students_list.SelectedIndex]["lesson_final"].ToString();
            }
        }

        private void Teacher_deny_Click(object sender, RoutedEventArgs e)
        {
            if (teacher_students_awaiting.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please select only one student!");
                return;
            }
            DataView view = (DataView)teacher_students_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            string cmdstr = "DELETE FROM tbl_lesson_notes WHERE lesson_student_email=@semail and lesson_name=@lname";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@lname", table.Rows[teacher_students_awaiting.SelectedIndex]["lesson_name"].ToString()),
                new dbConnection.cmdParameterType("@semail", table.Rows[teacher_students_awaiting.SelectedIndex]["lesson_student_email"].ToString().Trim().ToLower())
            };
            var delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);


            table.Rows.Remove(table.Rows[teacher_students_awaiting.SelectedIndex]);
            teacher_students_awaiting.ItemsSource = table.AsDataView();
        }

        private void Teacher_student_approve_Click(object sender, RoutedEventArgs e)
        {
            DataView view = (DataView)teacher_students_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            string cmdstr = "Update tbl_lesson_notes Set pending=0 where lesson_name=@lname and lesson_student_email=@semail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@lname", table.Rows[teacher_students_awaiting.SelectedIndex]["lesson_name"].ToString()),
                new dbConnection.cmdParameterType("@semail", table.Rows[teacher_students_awaiting.SelectedIndex]["lesson_student_email"].ToString().Trim().ToLower())
            };
            var approve = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            table.Rows.Remove(table.Rows[teacher_students_awaiting.SelectedIndex]);
            teacher_students_awaiting.ItemsSource = table.AsDataView();

            cmdstr = "Select lesson_name, lesson_student_email, lesson_student_name, lesson_student_surname, lesson_midterm, lesson_final from tbl_lesson_notes where lesson_teacher_email=@email and pending='0'";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@email", member_email.Content.ToString().Trim().ToLower())
            };
            var teacher_classes_approved = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            teacher_students_list.DataContext = teacher_classes_approved.AsDataView();

            MessageBox.Show("Request approved.");

        }

        private void Admin_teacher_approve_Click(object sender, RoutedEventArgs e)
        {
            if (admin_teacher_awaiting.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please approve 1 row at a time");
                return;
            }
            DataView view = (DataView)admin_teacher_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            
            view = (DataView)admin_registered_users.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view);

            table.Rows[admin_teacher_awaiting.SelectedIndex]["role"] = "2";
            table2.ImportRow(table.Rows[admin_teacher_awaiting.SelectedIndex]);

            string cmdstr = "Update tbl_users Set role=2 where email=@temail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@temail", table.Rows[admin_teacher_awaiting.SelectedIndex]["email"].ToString().Trim().ToLower())
            };
            var teacher_approved = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            table.Rows.Remove(table.Rows[admin_teacher_awaiting.SelectedIndex]);
            admin_teacher_awaiting.DataContext = table.AsDataView();
            admin_registered_users.DataContext = table2.AsDataView();

            MessageBox.Show("Success");
        }

        private void Admin_teacher_deny_Click(object sender, RoutedEventArgs e)
        {
            if (admin_teacher_awaiting.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please deny 1 row at a time");
                return;
            }
            DataView view = (DataView)admin_teacher_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            string cmdstr = "Update tbl_users Set role='denied' where email=@temail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@temail", table.Rows[admin_teacher_awaiting.SelectedIndex]["email"].ToString().Trim().ToLower())
            };
            var teacher_approved = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            table.Rows.Remove(table.Rows[admin_teacher_awaiting.SelectedIndex]);
            admin_teacher_awaiting.DataContext = table.AsDataView();

            MessageBox.Show("Success.");
        }

        private void Admin_lesson_approve_Click(object sender, RoutedEventArgs e)
        {
            if (admin_lessons_awaiting.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please approve 1 row at a time");
                return;
            }

            DataView view = (DataView)admin_lessons_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            view = (DataView)admin_lesson_list.ItemsSource;
            DataTable table2 = DataViewAsDataTable(view);

            table2.ImportRow(table.Rows[admin_lessons_awaiting.SelectedIndex]);

            string lesson_name = table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_name"].ToString().Trim().ToLower();

            string cmdstr = "Update tbl_lessons Set active=1 where lesson_name=@lname and lesson_teacher_email=@temail";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@temail", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_teacher_email"].ToString().ToLower().Trim()),
                new dbConnection.cmdParameterType("@lname", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_name"].ToString().Trim().ToLower()),

            };
            var lesson_approved = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

            


            cmdstr = "Delete from tbl_lessons where lesson_teacher_email!=@temail and lesson_name=@lname";
            lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@temail", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_teacher_email"].ToString().Trim().ToLower()),
                new dbConnection.cmdParameterType("@lname", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_name"].ToString().Trim().ToLower()),

            };
            var lesson_denied = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i]["lesson_name"].ToString().Trim().ToLower() == lesson_name)
                {
                    table.Rows.Remove(table.Rows[i]);
                }

            }

            admin_lesson_list.DataContext = table2.AsDataView();
            admin_lessons_awaiting.DataContext = table.AsDataView();
        }

        private void Admin_lesson_deny_Click(object sender, RoutedEventArgs e)
        {
            if (admin_lessons_awaiting.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please deny 1 row at a time");
                return;
            }

            DataView view = (DataView)admin_lessons_awaiting.ItemsSource;
            DataTable table = DataViewAsDataTable(view);

            var cmdstr = "Delete from tbl_lessons where lesson_teacher_email=@temail and lesson_name=@lname";
            var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
            {
                new dbConnection.cmdParameterType("@temail", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_teacher_email"].ToString().Trim().ToLower()),
                new dbConnection.cmdParameterType("@lname", table.Rows[admin_lessons_awaiting.SelectedIndex]["lesson_name"].ToString().Trim().ToLower()),

            };
            var lesson_denied = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
            table.Rows.Remove(table.Rows[admin_lessons_awaiting.SelectedIndex]);
            admin_lessons_awaiting.DataContext = table.AsDataView();
        }

        private void admin_lesson_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!wait)
            {
                DataView view = (DataView)admin_lesson_list.ItemsSource;
                DataTable table = DataViewAsDataTable(view);
                admin_lesson_name_new.Text = table.Rows[admin_lesson_list.SelectedIndex]["lesson_name"].ToString().Trim().ToLower();
            }
        }

        private void Admin_role_update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (admin_registered_users.SelectedItems.Count!=1)
            {
                MessageBox.Show("Please select only one user from registered users. ");
                return;
            }
            DataView view = (DataView)admin_registered_users.ItemsSource;
            DataTable table = DataViewAsDataTable(view);
            if (table.Rows[admin_registered_users.SelectedIndex]["role"].ToString()=="1")
            {
                MessageBox.Show("Can't change admins' role");
                return;
            }
            if (table.Rows[admin_registered_users.SelectedIndex]["role"].ToString()==(admin_update_role.SelectedIndex+2).ToString())
            {
                MessageBox.Show("User's role is already set to the selected role!");
                return;
            }

            if (MessageBox.Show("Are you sure? This decision would be catastrophic for other users who are related to selected user.\n(ex: students who participate in teacher's classes will drop from classes)", "This is irreversible!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                table.Rows[admin_registered_users.SelectedIndex]["role"] = (5 - Int32.Parse(table.Rows[admin_registered_users.SelectedIndex]["role"].ToString())).ToString();
                string selected_user_email = table.Rows[admin_registered_users.SelectedIndex]["email"].ToString();
                string cmdstr = $"Update tbl_users Set role={table.Rows[admin_registered_users.SelectedIndex]["role"]} where email=@email";
                var lstParamsQuickerWay = new List<dbConnection.cmdParameterType>
                {
                    new dbConnection.cmdParameterType("@email", selected_user_email),
                };
                var user_role_update = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                string msg = "";
                if (table.Rows[admin_registered_users.SelectedIndex]["role"].ToString() == "3")//former teacher
                {
                    cmdstr = "Delete from tbl_lessons where lesson_teacher_email=@email";
                    var lessons_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                    cmdstr= "Delete from tbl_lesson_notes where lesson_teacher_email=@email";
                    var lesson_notes_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                    cmdstr= $"Insert into tbl_students(email,registration_year,grade) values (@email,{DateTime.Now.Year},1)";
                    var user_add_to_students = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    admin_registered_users.DataContext = table.AsDataView();

                    view = (DataView)admin_lesson_list.ItemsSource;
                    table = DataViewAsDataTable(view);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if (table.Rows[i]["lesson_teacher_email"].ToString()== selected_user_email)
                        {
                            table.Rows.Remove(table.Rows[i]);
                            i--;
                        }
                    }
                    admin_lesson_list.DataContext = table.AsDataView();

                    view = (DataView)admin_lessons_awaiting.ItemsSource;
                    table = DataViewAsDataTable(view);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if (table.Rows[i]["lesson_teacher_email"].ToString() == selected_user_email)
                        {
                            table.Rows.Remove(table.Rows[i]);
                            i--;
                        }
                    }
                    admin_lessons_awaiting.DataContext = table.AsDataView();

                    msg =
                        "Deleted teacher's lessons, dropped students who participate on teacher's lessons, added student id and registration year to teacher";
                }
                else //former student
                {
                    cmdstr = "Delete from tbl_students where email=@email";
                    var student_info_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);
                    cmdstr = "Delete from tbl_lesson_notes where lesson_student_email=@email";
                    var student_lessons_delete = dbConnection.DB_Select(cmdstr, lstParamsQuickerWay);

                    msg = "Deleted student's id and removed student from participated classes. ";

                }

                MessageBox.Show("Done. Changed user's role.\n" + msg);
            }
        }

        private void Admin_department_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!wait && admin_department_combobox.SelectedIndex!=-1)
            {
                admin_department_name.Text =
                    admin_department_combobox.Items[admin_department_combobox.SelectedIndex].ToString();
            }
        }
    }
}
