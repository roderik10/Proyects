<?php

/* Crea el webservice para la clase Hangman */
require_once 'Hangman.php';

session_start();
$servidorSoap = new SoapServer('Hangman.wsdl');
$servidorSoap->setClass('Hangman');
$servidorSoap->setPersistence(SOAP_PERSISTENCE_SESSION);
$servidorSoap->handle();

?>