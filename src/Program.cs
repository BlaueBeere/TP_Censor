using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;

namespace ConsoleApplication1
{

    /// <summary>
    /// Text processing programm: Replaces an selected text in a text file through a censor-pattern.
    /// 
    /// The path of the text file and also the censor-pattern can be chosen by the user if he uses the following commandline structure:
    /// TP_CensorText.exe [relative path of the textfile] [censor-pattern]
    /// otherwise the following structure is chosen automatically:
    /// TP_CensorText.exe files\\document.txt XXXX
    /// </summary>
 
    class Program
    {

        static IEnumerable<string> SortByLength(IEnumerable<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.Length descending
                         select s;
            return sorted;
        }

        /// <summary>
        /// Replaces the keys in the original text at the path "path_to_original_doc"
        /// The keys are found in the string censored_keys and are separated by space and comma (words), single-/doublequotes(phrases).
        /// </summary>
        /// <param name="censored_keys">string with the censored keys inside</param>
        /// <param name="censor_pattern">the keys are replaced by this pattern</param>
        /// <param name="path_to_original_doc">path to the original document with the original text</param>
        /// <returns>the processed text</returns>
        static string TextProcessing_Censor_Text(string censored_keys, string censor_pattern, string path_to_original_doc)
        {
            //Var:
            string censor = censor_pattern;
            bool IS_PHRASE = false;
            bool IS_SINGLEPHRASE = false;
            List<string> keys = new List<string>();
            StringBuilder key_builder = new StringBuilder(200);
            char c;
            char next_c;
            int str_len = censored_keys.Length;
            string key;
            int i = 0;

            try
            {
                /*
                                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                                string rel_path = new Uri(Path.Combine(outPutDirectory, "files\\keywords.txt")).LocalPath;
                            Console.WriteLine("DIRRRRRR: " + rel_path);
                */

                //Get relative path of original document
                var replace_outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                string replace_path = new Uri(Path.Combine(replace_outPutDirectory, path_to_original_doc)).LocalPath;

                // using (StreamReader rd = new StreamReader(rel_path))
                using (StringReader rd = new StringReader(censored_keys))
                {
                    //Read from file until EOF
                    while (rd.Peek() >= 0)  // while (!inputReader.EndOfStream)
                    {
                        c = (char)rd.Read();
                        next_c = (char)rd.Peek();

                        //Select action based on char
                        switch (c)
                        {
                            case '\'':
                                //End of Phrase
                                if (IS_SINGLEPHRASE == true)
                                {
                                    //Reset Flag
                                    IS_SINGLEPHRASE = false;

                                    //Store key
                                    keys.Add(key_builder.ToString());
                                    key_builder.Clear();
                                }
                                else
                                {
                                    //Beginning of Phrase
                                    IS_SINGLEPHRASE = true;
                                }
                                break;
                            case '\"':
                                //End of Phrase
                                if (IS_PHRASE == true)
                                {
                                    //Reset Flag
                                    IS_PHRASE = false;

                                    //Store key
                                    keys.Add(key_builder.ToString());
                                    key_builder.Clear();
                                }
                                else
                                {
                                    //Beginning of Phrase, set flag
                                    IS_PHRASE = true;
                                }
                                break;
                            case ' ': 
                                //If phrase, append char
                                if (IS_PHRASE == true || IS_SINGLEPHRASE == true)
                                {
                                    key_builder.Append(' ');
                                }
                                break;
                            case ',':
                                //If phrase, append char
                                if (IS_PHRASE == true || IS_SINGLEPHRASE == true)
                                {
                                    key_builder.Append(',');
                                }
                                break;

                            default:
                                //Collect char
                                key_builder.Append(c);

                                ///If we´re not in a phrase:
                                if (IS_PHRASE == false && IS_SINGLEPHRASE == false)
                                {
                                    ///If next char is a delimiter or EOF
                                    if (next_c == ' ' || next_c == ',' || (rd.Peek() < 0))
                                    {
                                        //End of key, store key in array
                                        keys.Add(key_builder.ToString());
                                        key_builder.Clear();
                                    }
                                }
                                break;
                        }
                    }
                }

                ///Sort array of keywords (descending: long phrases, short words)
                foreach (string s in SortByLength(keys))
                {
                    keys[i] = s;
                }

                ///Replace keywords with selected censor
                for (i = 0; i < keys.Count; i++)
                {
                    key = keys[i];
                    File.WriteAllText(replace_path, Regex.Replace(File.ReadAllText(replace_path), key, censor));
                }

                return File.ReadAllText(replace_path);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                Console.ReadLine();
                throw;
            }
        }

        static void Main(string[] args)
        {
            ///Var:
            //string censored_keys = "asdf  \'qwertz\' jhjhjh \"aaaaa bbbb  qwertz\" asddf";
            string censored_keys = "Hello world \"Boston Red Sox\" Apple \"Power Pen\" \'Pepperoni Pizza\', \'Cheese Pizza\', beer, wine";
            string path_of_original_doc; // = "files\\document.txt";
            string processed_text;
            string censor;

            //Command-line input: specified path and censor-pattern
            if (args.Length == 2)
            {
                path_of_original_doc = "files\\" + args[0];
                censor = args[1];
            }
            else
            {//No command-line input
                path_of_original_doc = "files\\document.txt";
                censor = "XXXX";
            }

            Console.WriteLine("TPC: Text-Processing- Censor\n Changes the original text from " + path_of_original_doc 
                + ".\n Replaces the keywords <" + censored_keys + "> with a censor.");

            try
            {
                ///Process text, censor keys
                processed_text = TextProcessing_Censor_Text(censored_keys, censor, path_of_original_doc);
                ///Print the processed text
                Console.WriteLine("\n Processed text:");
                Console.WriteLine(processed_text);
                Console.WriteLine("End of Programm.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                Console.ReadLine();
            }
        }
    }
}

/*///Print the processed text from original file
using (StreamReader sr = new StreamReader(path_of_original_doc))
{
    while (sr.Peek() >= 0)
    {
        Console.Write((char)sr.Read());
    }
}*/