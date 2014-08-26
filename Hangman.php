<?php
ini_set("soap.wsdl_cache_enabled", 0);	//para mantener limpia la caché
include('adodb5/adodb.inc.php'); // incluye la biblioteca adodb
require_once('adodb5/adodb-active-record.inc.php');


/* Clase que implementa la lógica del juego */
class Hangman{


	public $maxAttmps = 10;	//cantidad máxima de intentos
	public $usrAttmps = 0;	//cantidad de intentos realizados
	public $acierto = 0;	//bandera
	public $startTime;		//tiempo de inicio de la partido
	public $endTime;		//tiempo final de la partida

	
	public function __construct(){
		$this->startTime = time();	
	}
	
/*
    Function Name: fetchWordArray() Escoge una palabra de la lista para jugar.
    Parameters: Nombre del archivo con las palabras.
    Return values: Palabra escogida para jugar.

*/
    public function fetchWordArray($wordFile)
    {
        $file = fopen($wordFile,'r');
           if ($file)
			{
				$random_line = null;
				$line = null;
				$count = 0;
				while (($line = fgets($file)) !== false) 
				{
					$count++;
					if(rand() % $count == 0) 
					{
						  $random_line = trim($line);
					}
				}
				if (!feof($file)) 
				{
					fclose($file);
					return null;
				}else 
				{
					fclose($file);
				}
			}
        $answer = str_split($random_line);
		$answerR = implode($answer);
        return $answerR;
    }
	

/*
    Function Name: hideCharacters() Reemplaza la palabra escogida con underscores.
    Parameters: Palabra a esconder.
    Return values: Palabra escondida.
*/
    public function hideCharacters($hideAns, $hidSize)
    {		
		for($i=0;$i<$hidSize;$i++){
			$hideAns = str_replace($hideAns[$i],'_',$hideAns);
		}
		
        return $hideAns;
    }
/*
    Function Name: checkAndReplace() Revisa si la letra ingresada es correcta, en dicho caso la sustituye en la palabra escondida.
    Parameters: Letra ingresada por el usuario, palabra escondida, respuesta, tamaño de la respuesta.
    Return values: Palabra escondida (modificada si fuera el caso).
*/
    public function checkAndReplace($userInput, $checkHidd, $checkAns, $checkSize)
    {
        $i = 0;
        $wrongGuess = true;
        while($i < $checkSize)
        {
            if ($checkAns[$i] == $userInput)
            {
                $checkHidd[$i] = $userInput;
                $wrongGuess = false;
            }
            $i = $i + 1;
        }
		$this->acierto = $this->usrAttmps;
        if ($wrongGuess) $this->usrAttmps = $this->usrAttmps + 1;

        return $checkHidd;
    }
    
    
/*
    Function Name: checkGameOver() Revisa el estado de la partida (victoria, derrota, intento fallido).
    Parameters: Respuesta, palabra escondida.

*/
    public function checkGameOver($gameAns, $gameAnsHidden)
    {
	
		if ($gameAns == $gameAnsHidden){
			$this->endTime = time();		
			return 1;  //juego ganado
			
		}else{
		
			if ($this->usrAttmps >= $this->maxAttmps){		
				return 0;  //juego perdido
			}else{
				if($this->acierto!=$this->usrAttmps){
					return -1;	//caracter fallado
				}else{
					return 10;  //caracter acertado
				}

				
			}
		}
	}
	
	
/*
    Function Name: getIntentos() Devuelve los intentos disponibles.
    Return values: Intentos disponibles.
*/
	public function getIntentos(){
		return $this->maxAttmps - $this->usrAttmps;
	}
	
/*
    Function Name: getTime() Devuelve el tiempo durado en la partida.
    Return values: Tiempo de la partida.
*/
	public function getTime(){
		return $this->endTime - $this->startTime;
	}
	
/*
    Function Name: select() Realiza una consulta a la BD para obtener el top ten.
    Return values: Conjunto de filas.
*/
	public function select(){
	
		$conn = &ADONewConnection('postgres');       //crea la conexión
		$conn->PConnect('localhost','eb02430','eb02430','ci2413');
		$recordSet = &$conn->Execute('select * from mejores_tiempos order by tiempo limit 10');
		$num = $recordSet->RecordCount();
		$rows = array();
		
		for($i=0;$i<$num;$i++){
			array_push($rows,$recordSet->fields[1]."-".$recordSet->fields[2]);
			$recordSet->MoveNext();
		}
		
		$recordSet->Close(); 
		$conn->Close(); 	//se cierra la conexión
		
		return $rows;
	}	
	
	
/*
    Function Name: insert() Inserta el usuario y su tiempo en la base de datos.
    Parameters: Usuario, tiempo de la partida.
*/
	public function insert($insertReq,$insertReq2){
	
		$conn = &ADONewConnection('postgres');       
		$conn->PConnect('localhost','eb02430','eb02430','ci2413');
		$recordSet = &$conn->Execute('select * from mejores_tiempos');
		$id = $recordSet->RecordCount() + 1;
		$sql = &$conn->Execute("insert into mejores_tiempos values('".$id."','".$insertReq."',".$insertReq2.")");
		
		$recordSet->Close(); 
		$conn->Close(); 
		
		
	}
}

?>