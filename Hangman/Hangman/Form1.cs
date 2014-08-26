using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Hangman
{

    //Clase cliente del juego
    public partial class Form1 : Form
    {

        private titanic.B02430_Hangman ws;  //instancia del servicio
        private System.Net.CookieContainer cookies; //cookies para el servidor
        private String answer;  //Respuesta de una partida
        private String hidden;  //String con la forma _ _ a _ _ que se va formando conforme se forma la respuesta
        private String usuario; //Jugador de la partida
        private String[,] topTen;   //Matriz para cargar la consulta de mejores tiempos desde la BD
        private String[] rows;  //Array auxiliar para cargar la consulta
        private int cont;   //contador para cargar las imágenes
        private int intentos;   //cantidad de intentos hecho por jugador

        /* 
        * Contructor de la clase
        */
        public Form1()
        {
            InitializeComponent();

            cont = 1;
        }

        /* 
        * Método que carga la ventana 
        */
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Load("http://titanic.ecci.ucr.ac.cr/~eb02430/TP2/hang/hang0.gif");  //método que carga la imagen inicial del ahorcado
            ws = new Hangman.titanic.B02430_Hangman();  //creación del servicio
            cookies = new System.Net.CookieContainer(); //creación del cookie
            ws.CookieContainer = cookies;   //asignación al servicio
            answer = ws.fetchWordArray("palabras.txt"); //escoge la palabra para el juego
            hidden = ws.hideCharacters(answer, answer.Length);  //esconde la palabra con _ _ _
            colocarResultado(hidden);   //coloca la palabra escondida en pantalla
            intentos = ws.getIntentos();    
            label3.Text = intentos.ToString();  //coloca los intentos disponibles en pantalla

            /* Deshabilita botones y campos de texto */
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            Top.Enabled = false;
            Probar.Enabled = false;
            label5.Visible = false;
            label6.Visible = false;
        }


        /* 
         * Método al presionar el botón de Probar 
         */
        private void button1_Click(object sender, EventArgs e)
        {

            String letraIngresada = " ";

            if(textBox2.Text.ToString()!=""){   //si el usuario no ingresa nada y oprime el botón, pierde un turno
                letraIngresada = textBox2.Text.ToString();
            }

            hidden = ws.checkAndReplace(letraIngresada, hidden, answer, answer.Length);   //revisa y reemplaza (si hubo acierto) el intento del jugador
            textBox2.Text = "";

            
            if(ws.checkGameOver(answer, hidden)==0){    //Juego perdido

                /* deshabilita botones y campos de texto */
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                Probar.Enabled = false;

                pictureBox1.Load("http://titanic.ecci.ucr.ac.cr/~eb02430/TP2/hang/hang10.gif"); 
                pictureBox2.Load("http://titanic.ecci.ucr.ac.cr/~eb02430/TP2/hang/hangFinal.jpg");  //carga imagen de pérdida

                rows = ws.select(); //select del top ten de mejores tiempos
                topTen = new String[rows.Length, 2];
                Top.Enabled = true; //se habilita la opción de mostrar el top ten
                intentos--;
                label5.Visible = true;
                label5.Text += "  Respuesta: " + answer;
            
            }else{

                colocarResultado(hidden);   //coloca palabra escondida en pantalla

                if (ws.checkGameOver(answer, hidden) == 1){     //Juego ganado

                    /* deshabilita botones y campos de texto */
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    Probar.Enabled = false;

                    ws.insert(usuario, ws.getTime());   //inserta el jugador y tiempo en la BD

                    pictureBox2.Load("http://titanic.ecci.ucr.ac.cr/~eb02430/TP2/hang/like.jpg");   //carga imagen ganadora

                    /* select a la BD y habilitación para ver el top ten */
                    rows = ws.select();
                    topTen = new String[rows.Length, 2];
                    Top.Enabled = true;
                    label6.Visible = true;
                }
                else {
                    if (ws.checkGameOver(answer, hidden) == -1) {   //Intento fallido
                        pictureBox1.Load("http://titanic.ecci.ucr.ac.cr/~eb02430/TP2/hang/hang"+cont+".gif");   //se carga la imagen formando el ahorcado
                        cont++; //para cargar la siguiente imagen
                        intentos--; //un intento menos por fallar                                        
                    }
                }               
            }

            label3.Text = intentos.ToString();  //actualización de los intentos en pantalla
        }


        /* Método auxiliar para cargar el resultado de la consulta a la BD. 
         * El resultado viene en un string. Se separa por columnas y se guarda para luego cargar en pantalla.
         */
        private void splitRows() {

            for(int i=0;i<rows.Length;i++){

                String[] x = rows[i].Split('-');

                for(int j=0;j<2;j++){

                    topTen[i, j] = x[j];
                }
            }
        }

        /*
         * Coloca el resultado parcial en pantalla
         */
        private void colocarResultado(String s) {

            textBox1.Text = "               ";
            for (int i = 0; i < s.Length; i++) {
                textBox1.Text += "  "+s[i];                
            }
        }
       



        /* 
         * Método que registra un jugador.
         */ 
        private void button2_Click_1(object sender, EventArgs e)
        {
            colocarResultado(hidden);
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            Probar.Enabled = true;
            textBox3.Enabled = false;
            Registrar.Enabled = false;

            usuario = textBox3.Text.ToString();

        }


        /*
         * Muestra el top ten de mejores tiempos
         */
        private void button3_Click(object sender, EventArgs e)
        {
            Top.Enabled = false;
            splitRows();
            DataTable table = new DataTable();

            /* Agrega columnas en el data table */
            table.Columns.Add("Jugador");
            table.Columns.Add("Tiempo en segundos");

            // Para cada columna
            for (int i = 0; i < 2; i++)
            {
                //Lista de objects
                List<object> objectRows = new List<object>();

                //Convierte las filas para la columna en objects
                for (int k = 0; k < rows.Length; k++) {
                    objectRows.Add((object)topTen[k, i]);
                }


                // Agrega las filas (visuales) en el data table
                while (table.Rows.Count < objectRows.Count)
                {
                    table.Rows.Add();
                }

                // Agrega los datos de las filas en el data table
                for (int a = 0; a < objectRows.Count; a++)
                {
                    table.Rows[a][i] = objectRows[a];
                }
            }

            dataGridView1.DataSource = table; //Agrega el data table en el datagrid
            
        }

    }
}
