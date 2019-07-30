using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;

namespace C_Sharp_DB_LINQ
{
    class Program
    {
     

             public static SqlConnection GetConnection()
        {
            //method to return a SqlConnection
            //string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=c:\users\johnn\documents\visual studio 2015\Projects\OLA7JW\StudentDB.mdf;Integrated Security=True";
              string connStr = @"Data Source =(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\johnn\Documents\Visual Studio 2015\Projects\OLA7JohnWestbrooks\StudentDB.mdf;Integrated Security = True; Connect Timeout = 30";
            SqlConnection conn = new SqlConnection(connStr);
            return conn;
        }

        public static void AddEntries(string lastName, string firstName, string year, int hours, float gpa, string course, string courseID, string grade, int csHours, float csGPA)
        {
            //method to add entry to database
            bool flag = false;
            string insStmt = "INSERT INTO Students (LastName, FirstName, Class, EarnedHours, GPA, Course, CourseNumber, FinalGrade, EarnedCSCIHours, CSCIGPA) VALUES (@lastName, @firstName, @year, @hours, @gpa, @course, @courseID, @grade, @csHours, @csGPA)";
            SqlConnection conn = GetConnection();  //call connection method
            SqlCommand insCmd = new SqlCommand(insStmt, conn);

            insCmd.Parameters.AddWithValue("@lastName", lastName);
            insCmd.Parameters.AddWithValue("@firstName", firstName);
            insCmd.Parameters.AddWithValue("@year", year);
            insCmd.Parameters.AddWithValue("@hours", hours);
            insCmd.Parameters.AddWithValue("@gpa", gpa);
            insCmd.Parameters.AddWithValue("@course", course);
            insCmd.Parameters.AddWithValue("@courseID", courseID);
            insCmd.Parameters.AddWithValue("@grade", grade);
            insCmd.Parameters.AddWithValue("@csHours", csHours);
            insCmd.Parameters.AddWithValue("@csGPA", csGPA);

            try
            {
                conn.Open();
                insCmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                flag = true;
                if (e.InnerException != null) //show inner exception if there is one
                {
                    Console.WriteLine("Inner Exception: ");
                    Console.WriteLine(e.InnerException.Message.ToString());
                }
                Console.WriteLine("Outer Exception: "); //show exception message
                Console.WriteLine(e.Message.ToString());
                throw e;
            }
            finally
            {
                conn.Close();
                if (flag)
                {
                    Console.WriteLine("Program halting.");
                }
            }
        }//end AddEntries



        static void Main(string[] args)
        {
            //This block is the code used to intialize database.
            try
            {                         //open file and reader
                using (StreamReader sReader = File.OpenText("csvDBData.txt"))
                {
                    string txt;
                    while ((txt = sReader.ReadLine()) != null)                 //keep reading until EoF
                    {
                        int hours2;
                        float gpa2;
                        string[] spltTxt = txt.Split(',');                      //Split the current line into distinct entries
                        int hours, csHours;
                        float gpa, csGPA;
                        string lastName = spltTxt[0];
                        string firstName = spltTxt[1];
                        string year = spltTxt[2];
                        string course = spltTxt[5];
                        string courseID = spltTxt[6];
                        string grade = spltTxt[7];
                        string convert;
                        convert = spltTxt[3];                                    //var for TryParse call

                        if (spltTxt[3] == "")
                            hours = 0;
                        else
                        {
                            bool result = Int32.TryParse(convert, out hours2); //int parse did not work
                            if (result)                                         //if conversion successful, set to conversion
                                hours = hours2;
                            else
                                hours = 0;                                      //else give it place holder of 0
                        }

                        if (spltTxt[4] == "")
                            gpa = 0;
                        else
                            gpa = float.Parse(spltTxt[4]);

                        if (spltTxt[8] == "")
                            csHours = 0;
                        else
                            csHours = Int32.Parse(spltTxt[8]);

                        if (spltTxt[9] == "")
                            csGPA = 0;
                        else
                        {
                            bool result2 = float.TryParse(convert, out gpa2); //float parse did not work
                            if (result2)
                                csGPA = gpa2;
                            else
                                csGPA = 0;
                        }

                        AddEntries(lastName, firstName, year, hours, gpa, course, courseID, grade, csHours, csGPA);
                    }//end while
                }//end using - SR is dismissed
            } //end try
            catch (Exception e)
            {
                Console.WriteLine("Exception Message: ");
                Console.WriteLine(e.Message.ToString());
                Console.WriteLine("Inner Message: ");
                Console.WriteLine(e.InnerException.ToString());
            }


            try
            {
                //create context
                using (StudentDBEntities DBContext = new StudentDBEntities())
                {

                    //query the database with LINQ - group the students of each class
                    //by their distinct identifying traits

                    var freshmanFiltered = from s in DBContext.Students
                                               //put all freshman students into a list
                                           where s.Class == "FR"
                                           //group  each entry in list into a sublist by their distinct characteristics
                                           group s by new { s.LastName, s.FirstName, s.GPA, s.CSCIGPA, s.EarnedHours, s.EarnedCSCIHours, s.Class } into s2
                                           //order outer list by GPA, and then department GPA
                                           orderby s2.Key.GPA descending, s2.Key.GPA descending
                                           select s2;

                    //repeat process for other classes
                    var sophemoreFiltered = from s in DBContext.Students
                                            where s.Class == "SO"
                                            group s by new { s.LastName, s.FirstName, s.GPA, s.CSCIGPA, s.EarnedHours, s.EarnedCSCIHours, s.Class } into s2
                                            orderby s2.Key.GPA descending, s2.Key.GPA descending
                                            select s2;


                    var juniorFiltered = from s in DBContext.Students
                                         where s.Class == "JR"
                                         group s by new { s.LastName, s.FirstName, s.GPA, s.CSCIGPA, s.EarnedHours, s.EarnedCSCIHours, s.Class } into s2
                                         orderby s2.Key.GPA descending, s2.Key.GPA descending
                                         select s2;


                    var seniorFiltered = from s in DBContext.Students
                                         where s.Class == "SR"
                                         group s by new { s.LastName, s.FirstName, s.GPA, s.CSCIGPA, s.EarnedHours, s.EarnedCSCIHours, s.Class } into s2
                                         orderby s2.Key.GPA descending, s2.Key.GPA descending
                                         select s2;


                    Console.WriteLine("Displaying student information by class (year):");
                    Console.WriteLine("NOTE: A 0 or blank space indicates data for this field was not provided.");

                    //Display query results
                    Console.WriteLine("\n\nFRESHMAN CLASS: ");


                    foreach (var stud in freshmanFiltered)
                    {
                        //outer loop access the elements' of the outer list's keys
                        Console.Write("{0}, ", stud.Key.LastName);
                        Console.Write("{0}, ", stud.Key.FirstName);
                        Console.Write("{0}, ", stud.Key.Class);
                        Console.Write("{0}, ", stud.Key.EarnedHours);
                        Console.Write("{0}, ", stud.Key.GPA);
                        foreach (var stud2 in stud)
                        {
                            //inner loop acccess the distinct student objects
                            Console.Write(stud2.Course);
                            Console.Write(stud2.FinalGrade);
                            Console.Write(", ");
                        }
                        Console.Write("{0}, {1}\n", stud.Key.EarnedCSCIHours, stud.Key.CSCIGPA);
                    }
                    //repeat process for other classes
                    Console.WriteLine("\n\nSOPHMORE CLASS: ");
                    foreach (var stud in sophemoreFiltered)
                    {
                        Console.Write("{0}, ", stud.Key.LastName);
                        Console.Write("{0}, ", stud.Key.FirstName);
                        Console.Write("{0}, ", stud.Key.Class);
                        Console.Write("{0}, ", stud.Key.EarnedHours);
                        Console.Write("{0}, ", stud.Key.GPA);
                        foreach (var stud2 in stud)
                        {
                            Console.Write(stud2.Course);
                            Console.Write(stud2.FinalGrade);
                            Console.Write(", ");
                        }
                        Console.Write("{0}, {1}\n", stud.Key.EarnedCSCIHours, stud.Key.CSCIGPA);
                    }

                    Console.WriteLine("\n\nJUNIOR CLASS: ");
                    foreach (var stud in juniorFiltered)
                    {
                        Console.Write("{0}, ", stud.Key.LastName);
                        Console.Write("{0}, ", stud.Key.FirstName);
                        Console.Write("{0}, ", stud.Key.Class);
                        Console.Write("{0}, ", stud.Key.EarnedHours);
                        Console.Write("{0}, ", stud.Key.GPA);
                        foreach (var stud2 in stud)
                        {
                            Console.Write(stud2.Course);
                            Console.Write(stud2.FinalGrade);
                            Console.Write(", ");
                        }
                        Console.Write("{0}, {1}\n\n", stud.Key.EarnedCSCIHours, stud.Key.CSCIGPA);
                    }

                    Console.WriteLine("\n\nSENIOR CLASS: ");
                    foreach (var stud in seniorFiltered)
                    {
                        Console.Write("{0}, ", stud.Key.LastName);
                        Console.Write("{0}, ", stud.Key.FirstName);
                        Console.Write("{0}, ", stud.Key.Class);
                        Console.Write("{0}, ", stud.Key.EarnedHours);
                        Console.Write("{0}, ", stud.Key.GPA);
                        foreach (var stud2 in stud)
                        {
                            Console.Write(stud2.Course);
                            Console.Write(stud2.FinalGrade);
                            Console.Write(", ");
                        }
                        Console.Write("{0}, {1}\n\n", stud.Key.EarnedCSCIHours, stud.Key.CSCIGPA);
                    }
                }//end using (2)
            }//end try   (2)
            catch (Exception e)
            {
                Console.WriteLine("Exception Message: ");
                Console.WriteLine(e.ToString());
                Console.WriteLine("Inner Message: ");
                Console.WriteLine(e.InnerException.ToString());
            }
            finally
            {
                Console.WriteLine("Program halting.");
            }
            Console.Read(); //pause program
        }//end main



    }
    }

