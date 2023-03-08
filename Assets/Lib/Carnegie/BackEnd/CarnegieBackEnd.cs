using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

/// <summary>
/// This is the example class defined for sending AA Request and Audio Data following the speechservices API documents from Carnegie Speech Company Inc.
/// The class was developed to provide the customer an example of how to make an audio analysis transaction to speechservices server of Carnegie Speech Company Inc.
/// The efficiency and robustness of this code were tested but not in high priority during the develoment of this class. 
/// The lcustomer's own development and testing are strongly suggested. 
/// </summary>
/// 

namespace Carnegie
{
	public class CarnegieBackEnd
	{
        static string verID = "SendFileClass_CS_v1.0.3";
        TcpClient tcpClient = null;
        NetworkStream ns;
        BinaryWriter bw;
        string errorMsg = null;
	
        public CarnegieBackEnd(string srvAddress, int portNum)
        {
            //
            // TODO: Add constructor logic here
            //
            try
            {
                tcpClient = new TcpClient(srvAddress, portNum);
                ns = tcpClient.GetStream();
                bw = new BinaryWriter(ns);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void Close()
        {
            try
            {
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.StackTrace);
            }
        }

        public Boolean sendAARequest(string transactionID, string validationString, string aarequestString, string audioType)
        {
            try
            {
                Int32 hdrLen, bdyLen = 0;
                string msgString = null;
                /*modify the aarequestString*/
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(aarequestString);
                doc.DocumentElement.SetAttribute("AAReqID", transactionID);
                doc.DocumentElement.GetElementsByTagName("analysis")[0].Attributes[0].Value = transactionID;
                
                /*Forming the header string of AA request message*/
                msgString = "<message serviceType='AARequest' transactionID='" + transactionID + "'>\n" + validationString + "\n" +
                    "<audio type='" + audioType + "' save='True'/>\n" + doc.OuterXml + "\n</message>";
                byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                hdrLen = (Int32)msgBuff.Length;

                bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                bw.Flush();
                ns.Write(msgBuff, 0, msgBuff.Length);
                ns.Flush();
                return true;
            }
            catch (Exception e)
            {
                errorMsg += "Exception happened in sending AA Request Message\n";
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
        }

        /*Send Audio Block Message with specified data rate*/
        /* Send the whole aduio file using just one Audio Block Message with specified data rate*/
        public Boolean sendAudiofileInOneMsg(string transactionID, string fileName, string dataRate)
        {
            if (null == dataRate)
            {
                Boolean returnValue = sendAudiofileInOneMsg(transactionID, fileName);
                return returnValue;
            }
            else
            {
                string hdrString;
                string adkString;
                string msgString;
                Int32 dRate = Int32.Parse(dataRate);
                long fileSize;
                Int32 hdrLen, bdyLen;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                fileSize = new FileInfo(fileName).Length;
                byte[] audBuffer = br.ReadBytes((int)fileSize);
                long totalTime = (long)(((float)fileSize / dRate) * 1000);
                br.Close();

                hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final=\'true\'>\n";
                adkString = "<AudioBlock AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
                msgString = hdrString + adkString;
                hdrLen = msgString.Length;
                bdyLen = (Int32)fileSize;
                try
                {
                    /*Send Header Message String*/
                    byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                    long startTime = System.DateTime.Now.Millisecond;
                    bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                    bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                    bw.Flush();
                    ns.Write(msgBuff, 0, msgBuff.Length);
                    ns.Flush();
                    /*Start to send audio samples*/
                    ns.Write(audBuffer, 0, audBuffer.Length);
                    long endTime = System.DateTime.Now.Millisecond;
                    long elapsedTime = endTime - startTime;
                    if ((totalTime - elapsedTime) > 0)
                    {
                        Thread.Sleep((int)(totalTime - elapsedTime));
                    }
                    return true;
                }
                catch (IOException e)
                {
                    errorMsg += "IOException happened in sending Audio Block Message\n";
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
                catch (Exception ee)
                {
                    errorMsg += "Exception happened in sending Audio Block Message\n";
                    Console.WriteLine(ee.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
            }
        }

        /*Send Audio Block Message*/
        /* Send the whole aduio file using just one Audio Block Message*/
        public Boolean sendAudiofileInOneMsg(string transactionID, string fileName)
        {
            string hdrString;
            string adkString;
            string msgString;
            Int32 hdrLen, bdyLen;
            long fileSize;
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fileSize = new FileInfo(fileName).Length;
            byte[] audBuffer = br.ReadBytes((int)fileSize);
            br.Close();

            hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final=\'true\'>\n";
            adkString = "<AudioBlock AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
            msgString = hdrString + adkString;

            bdyLen = (Int32)fileSize;
            try
            {
                /*Send Header Message String*/
                byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                hdrLen = msgBuff.Length;

                bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                bw.Flush();
                ns.Write(msgBuff, 0, msgBuff.Length);
                ns.Flush();
                /*Start to send audio samples*/
                ns.Write(audBuffer, 0, audBuffer.Length);
                return true;
            }
            catch (IOException e)
            {
                errorMsg += "IOException happened in sending Audio Block Message\n";
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
            catch (Exception ee)
            {
                errorMsg += "Exception happened in sending Audio Block Message\n";
                Console.WriteLine(ee.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
        }

        /*Send Audio Block Message*/
        /* Send the audio bytes in a buffer using just one Audio Block Message with specific data rate*/
        public Boolean sendAudiofileInOneMsg(string transactionID, byte[] audioBuffer, string dataRate)
        {
            if (null == dataRate)
            {
                Boolean returnValue = sendAudiofileInOneMsg(transactionID, audioBuffer);
                return returnValue;
            }
            else
            {
                string hdrString;
                string adkString;
                string msgString;
                Int32 dRate = Int32.Parse(dataRate);

                Int32 hdrLen, bdyLen;

                long totalTime = (long)(((float)audioBuffer.Length / dRate) * 1000);


                hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final=\'true\'>\n";
                adkString = "<AudioBlock AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
                msgString = hdrString + adkString;
                hdrLen = msgString.Length;
                bdyLen = (Int32)audioBuffer.Length;
                try
                {
                    /*Send Header Message String*/
                    byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                    long startTime = System.DateTime.Now.Millisecond;
                    bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                    bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                    bw.Flush();
                    ns.Write(msgBuff, 0, msgBuff.Length);
                    ns.Flush();
                    /*Start to send audio samples*/

                    ns.Write(audioBuffer, 0, audioBuffer.Length);
                    long endTime = System.DateTime.Now.Millisecond;
                    long elapsedTime = endTime - startTime;
                    if ((totalTime - elapsedTime) > 0)
                    {
                        Thread.Sleep((int)(totalTime - elapsedTime));
                    }
                    return true;
                }
                catch (IOException e)
                {
                    errorMsg += "IOException happened in sending Audio Block Message\n";
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
                catch (Exception ee)
                {
                    errorMsg += "Exception happened in sending Audio Block Message\n";
                    Console.WriteLine(ee.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
            }
        }
        /*Send Audio Block Message*/
        /* Send the audio bytes in a buffer using just one Audio Block Message*/
        public Boolean sendAudiofileInOneMsg(string transactionID, byte[] audioBuffer)
        {
            string hdrString;
            string adkString;
            string msgString;
            Int32 hdrLen, bdyLen;

            hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final=\'true\'>\n";
            adkString = "<AudioBlock AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
            msgString = hdrString + adkString;
            hdrLen = msgString.Length;
            bdyLen = (Int32)audioBuffer.Length;
            try
            {
                /*Send Header Message String*/
                byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                bw.Flush();
                ns.Write(msgBuff, 0, msgBuff.Length);
                ns.Flush();
                /*Start to send audio samples*/
                ns.Write(audioBuffer, 0, audioBuffer.Length);
                return true;
            }
            catch (IOException e)
            {
                errorMsg += "IOException happened in sending Audio Block Message\n";
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
            catch (Exception ee)
            {
                errorMsg += "Exception happened in sending Audio Block Message\n";
                Console.WriteLine(ee.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
        }
        
        /*Send Audio Block Message*/
        /*Send audio file in several Audio Block Messages, with specific data rate*/
        public Boolean sendAudiofileInMultiMsg(string transactionID, string fileName, int blockLen, string dataRate)
        {
            if (null == dataRate)
            {
                Boolean returnValue = sendAudiofileInMultiMsg(transactionID, fileName, blockLen);
                return returnValue;
            }
            else
            {
                /*Each Audio Block Message contains "blklen" Bytes of audio data, except the final Audio Block Message*/
                string hdrString;
                string adkString;
                string msgString;
                int dRate = int.Parse(dataRate);
                long fileSize;
                int msgNum;
                int lftByteNum;
                Int32 hdrLen, bdyLen;
                long totalTime;
                long startTime, endTime, elapsedTime;

                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                fileSize = new FileInfo(fileName).Length;
                byte[] audBuffer = br.ReadBytes((int)fileSize);
                br.Close();

                msgNum = (int)fileSize / blockLen;//Calculate how many Audio Block Messages needed for sending this audio file.

                try
                {
                    lftByteNum = (int)fileSize % blockLen;//Calculate how many Bytes of data for the last Audio Block Message
                    if (((0 == lftByteNum) && (1 == msgNum)) || (lftByteNum == fileSize))
                    {
                        //If the audio file size is less than the "blklen", use only one Audio Block Message to send
                        Boolean returnValue = sendAudiofileInOneMsg(transactionID, fileName, dataRate);
                        return returnValue;
                    }
                    /*Preparing the message string*/
                    hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final = \'false\'>\n";
                    adkString = "<AudioBlock  AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
                    msgString = hdrString + adkString;

                    /*Preparing pre-amble values*/
                    byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                    hdrLen = msgBuff.Length;
                    byte[] blockBuff = new byte[blockLen];
                    for (int j = 0; j < msgNum; j++)
                    {
                        Buffer.BlockCopy(audBuffer, j * blockLen, blockBuff, 0, blockLen);
                        bdyLen = blockLen; //Calculate how many bytes of the body of Audio Block Message

                        /*Sending Pre-amble, header message*/
                        totalTime = (long)(((float)blockLen / dRate) * 1000);
                        hdrLen = msgBuff.Length;
                        startTime = System.DateTime.Now.Millisecond;
                        bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                        bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                        bw.Flush();
                        ns.Write(msgBuff, 0, msgBuff.Length);
                        ns.Flush();
                        /*Start to send audio samples*/
                        ns.Write(blockBuff, 0, blockBuff.Length);
                        endTime = System.DateTime.Now.Millisecond;
                        elapsedTime = endTime - startTime;
                        if ((totalTime - elapsedTime) > 0)
                        {
                            Thread.Sleep((int)(totalTime - elapsedTime));
                        }
                    }
                    /*Sending the last Audio Block Message*/
                    hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final = \'true\'>\n";
                    msgString = hdrString + adkString;
                    msgBuff = Encoding.UTF8.GetBytes(msgString);
                    hdrLen = msgBuff.Length; //Calculate how many bytes the header has
                    bdyLen = lftByteNum;
                    totalTime = (long)(((float)lftByteNum / dRate) * 1000);
                    Buffer.BlockCopy(audBuffer, msgNum * blockLen, blockBuff, 0, lftByteNum);

                    startTime = System.DateTime.Now.Millisecond;
                    bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                    bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                    bw.Flush();
                    ns.Write(msgBuff, 0, msgBuff.Length);
                    ns.Flush();
                    /*Start to send audio samples*/
                    ns.Write(blockBuff, 0, blockBuff.Length);
                    endTime = System.DateTime.Now.Millisecond;
                    elapsedTime = endTime - startTime;
                    if ((totalTime - elapsedTime) > 0)
                    {
                        Thread.Sleep((int)(totalTime - elapsedTime));
                    }
                    return true;
                }
                catch (IOException e)
                {
                    errorMsg += "IOException happened in sending Audio Block Message\n";
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
                catch (Exception ee)
                {
                    errorMsg += "Exception happened in sending Audio Block Message\n";
                    Console.WriteLine(ee.StackTrace);
                    Console.WriteLine(errorMsg);
                    return false;
                }
            }
        }

        /*Send Audio Block Message*/
        /*Send audio file in several Audio Block Messages*/
        public Boolean sendAudiofileInMultiMsg(string transactionID, string fileName, int blockLen)
        {
            /*Each Audio Block Message contains "blklen" Bytes of audio data, except the final Audio Block Message*/
            string hdrString;
            string adkString;
            string msgString;
            long fileSize;
            int msgNum;
            int lftByteNum;
            Int32 hdrLen, bdyLen;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fileSize = new FileInfo(fileName).Length;
            byte[] audBuffer = br.ReadBytes((int)fileSize);
            br.Close();

            msgNum = (int)fileSize / blockLen;//Calculate how many Audio Block Messages needed for sending this audio file.

            try
            {
                lftByteNum = (int)fileSize % blockLen;//Calculate how many Bytes of data for the last Audio Block Message
                if (((0 == lftByteNum) && (1 == msgNum)) || (lftByteNum == fileSize))
                {
                    //If the audio file size is less than the "blklen", use only one Audio Block Message to send
                    Boolean returnValue = sendAudiofileInOneMsg(transactionID, fileName);
                    return returnValue;
                }
                /*Preparing the message string*/
                hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final = \'false\'>\n";
                adkString = "<AudioBlock  AAReqID=\'" + transactionID + "\'>\n</AudioBlock>\n</message>";
                msgString = hdrString + adkString;

                /*Preparing pre-amble values*/
                byte[] msgBuff = Encoding.UTF8.GetBytes(msgString);
                hdrLen = msgBuff.Length;
                byte[] blockBuff = new byte[blockLen];
                for (int j = 0; j < msgNum; j++)
                {
                    Buffer.BlockCopy(audBuffer, j * blockLen, blockBuff, 0, blockLen);
                    hdrLen = msgBuff.Length;
                    bdyLen = blockLen; //Calculate how many bytes of the body of Audio Block Message

                    /*Sending Pre-amble, header message*/
                    bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                    bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                    bw.Flush();
                    ns.Write(msgBuff, 0, msgBuff.Length);
                    ns.Flush();
                    /*Start to send audio samples*/
                    ns.Write(blockBuff, 0, blockBuff.Length);
                }
                /*Sending the last Audio Block Message*/
                hdrString = "<message serviceType=\'AudioBlock\' transactionID=\'" + transactionID + "\' final = \'true\'>\n";
                msgString = hdrString + adkString;
                msgBuff = Encoding.UTF8.GetBytes(msgString);
                hdrLen = msgBuff.Length; //Calculate how many bytes the header has
                bdyLen = lftByteNum;
                Buffer.BlockCopy(audBuffer, msgNum * blockLen, blockBuff, 0, lftByteNum);

                bw.Write(IPAddress.HostToNetworkOrder(hdrLen));
                bw.Write(IPAddress.HostToNetworkOrder(bdyLen));
                bw.Flush();
                ns.Write(msgBuff, 0, msgBuff.Length);
                ns.Flush();
                /*Start to send audio samples*/
                ns.Write(blockBuff, 0, lftByteNum);
                return true;
            }
            catch (IOException e)
            {
                errorMsg += "IOException happened in sending Audio Block Message\n";
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
            catch (Exception ee)
            {
                errorMsg += "Exception happened in sending Audio Block Message\n";
                Console.WriteLine(ee.StackTrace);
                Console.WriteLine(errorMsg);
                return false;
            }
        }

        /*This function is to receive the AA Result Message from Server, and save the received message into a local folder*/
        public string recvAAresult()
        {
            int strLen;
            Int32 hdrLen, bdyLen;
            string recvString = null;

            /*make a local file name for saving AA Result Message*/
            try
            {
                BinaryReader br = new BinaryReader(ns);
                /*Start to read AA Result Message, "dis" is the DataInputStream through Socket*/
                //MesgLen = dis.readInt();// Read a 32-bit Integer as message length
                //System.out.println("MsgLen: "+MesgLen);
                hdrLen = IPAddress.NetworkToHostOrder(br.ReadInt32());// Read a 32-bit Integer as header length
                bdyLen = IPAddress.NetworkToHostOrder(br.ReadInt32());// Read a 32-bit Integer as body length


                strLen = hdrLen + bdyLen; //Calculate how many Bytes the message string is within AA Result Message

                byte[] recvBytes = new byte[strLen];
                recvBytes = br.ReadBytes(strLen);

                recvString = System.Text.Encoding.UTF8.GetString(recvBytes);

                /*Start to write the received message to the local file*/
            }
            catch (IOException e)
            {
                errorMsg += "IOException happened in receiving AA Result Message\n";
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(errorMsg);
                return errorMsg;
            }
            catch (Exception ee)
            {
                errorMsg += "Exception happened in receiving AA Result Message\n";
                Console.WriteLine(ee.StackTrace);
                Console.WriteLine(errorMsg);
                return errorMsg;
            }
            return recvString;
        }

        public static string generateValidationString(string readString, string clientSecretKey)
        {
            /*Read XML-style auth string */
            if ((null == clientSecretKey) || (null == readString))
            {
                string validationString = null;
                return validationString;
            }
            
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(readString);

                string requester = doc.DocumentElement.GetAttribute("requester").ToString();
                string requesterID = doc.DocumentElement.GetAttribute("requesterID").ToString();
                string requesterInfo = doc.DocumentElement.GetAttribute("requesterInfo").ToString();
                string requesterSeqNum = doc.DocumentElement.GetAttribute("requesterSeqNum").ToString();
                string datePatt = @"yyyy-MM-dd'T'HH:mm:ss.fffzzz";
                string dateTimeStamp = DateTime.Now.ToString(datePatt);
                Console.WriteLine(dateTimeStamp);
                string hashTokenInstance = requester + "-" + requesterID + "-" + requesterInfo + "-" + dateTimeStamp + "-" + requesterSeqNum + "-" + clientSecretKey;

                MD5 md5Hash = MD5.Create();
                string hashString = GetMd5Hash(md5Hash, hashTokenInstance);
                string validationString = "   <validate requester='" + requester + "'\n" +
                                              "       requesterID='" + requesterID + "'\n" +
                                              "       requesterInfo='" + requesterInfo + "'\n" +
                                              "       requesterSeqNum='" + requesterSeqNum + "'\n" +
                                              "       dateTimeStamp='" + dateTimeStamp + "'\n" +
                                              "       hashToken='" + hashString + "'/>\n";

                return validationString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                string validationString = null;
                return validationString;
            }
        }
        /*@overload
         */
        public static string generateValidationString(
            string requester,
            string requesterID,
            string userID,
            string requesterInfo,
            int requesterSeqNum,
            string clientSecretKey)
        {
            if ((null == clientSecretKey))
            {
                string validationString = null;
                return validationString;
            }
            
            try
            {
                string datePatt = @"yyyy-MM-dd'T'HH:mm:ss.fffzzz";
                string dateTimeStamp = DateTime.Now.ToString(datePatt);

                string hashTokenInstance = requester + "-" + requesterID + "-" + requesterInfo + "-" + dateTimeStamp + "-" + requesterSeqNum + "-" + clientSecretKey;
                Console.WriteLine(hashTokenInstance);
                MD5 md5Hash = MD5.Create();
                string hashString = GetMd5Hash(md5Hash, hashTokenInstance);
                string validationString = "   <validate apiVersion='" + verID + "'\n" +
                                              "       requester='" + requester + "'\n" +
                                              "       requesterID='" + requesterID + "'\n" +
                                              "       userID='" + userID + "'\n" +
                                              "       requesterInfo='" + requesterInfo + "'\n" +
                                              "       requesterSeqNum='" + requesterSeqNum + "'\n" +
                                              "       dateTimeStamp='" + dateTimeStamp + "'\n" +
                                              "       hashToken='" + hashString + "'/>\n";

                return validationString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                string validationString = null;
                return validationString;
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        /* 
         * Used for making a single AA transaction reading in audio data from an audio bytes buffer and sending with a specified data rate.
         * 
         * @param XML-style authorization string
         * 		Example: <validate requester="CSDev" requesterID="21" requesterInfo='user1' requesterSeqNum='936864273622677291'/>
         * @param XML-style AA request string
         * 		Example: <AARequest AAReqID="201"><analysis analysisID="201" analysisType="ReadingFluency"  ><text>Will you help me with the VAST chip security phrases?</text></analysis></AARequest>
         * @param bytes buffer containing the bytes of audio
         * @param secrectKey for client
         * @param transaction ID for this single transaction
         * @param a string stating audio file format
         * @param a string stating the data rate: # of bytes per second, string must be string of a positive integer or null
         * @return AA result string sent from speechservices server
         */
        public string singleAATransaction(string authString, string aareqString, byte[] audioBuffer, string clientSecretKey, string transactionID, string audioType, string dataRate)
        {
            if (null == dataRate)
            {
                string returnValue = singleAATransaction(authString, aareqString, audioBuffer, clientSecretKey, transactionID, audioType);
                return returnValue;
            }
            string errorMsg = null;
            string validationString = null;
            string aarequestString = null;
            /*Start to Check Inputs*/
            if (Int32.Parse(dataRate) <= 0)
            {
                errorMsg += "Datarate string must be string of a positive integer \n";
            }
            validationString = generateValidationString(authString, clientSecretKey);
            if (null == validationString)
            {
                errorMsg += "Failed in generatin validation string, please check your auth string\n";
            }
            if (null == aareqString)
            {
                errorMsg += "aarequest string should not be empty\n";
            }
            if (audioBuffer.Length <= 0)
            {
                errorMsg += "Audio buffer should not be empty\n";
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(audioType, @"(?i)^[a-zA-Z]+$"))
            {
                errorMsg += "audio file type string should be all letters\n";
            }
            if (null == transactionID)
            {
                errorMsg += "Transaction ID should not be null\n";
            }
            /*End of Inputs Checking*/
            if (null != errorMsg)
            {
                return errorMsg;
            }
            aarequestString = aareqString;
            this.sendAARequest(transactionID, validationString, aarequestString, audioType);
            try
            {
                this.sendAudiofileInOneMsg(transactionID, audioBuffer, dataRate);
            }
            catch (FileNotFoundException e)
            {
                errorMsg += "file not found exception\n";
                Console.WriteLine(e.StackTrace);
                return errorMsg;
            }
            string aaresult = this.recvAAresult();
            return aaresult;
        }
         /* 
         * Used for making a single AA transaction reading in audio data from an audio bytes buffer.
         * 
         * @param XML-style authorization string
         * 		Example: <validate requester="CSDev" requesterID="21" requesterInfo='user1' requesterSeqNum='936864273622677291'/>
         * @param XML-style AA request string
         * 		Example: <AARequest AAReqID="201"><analysis analysisID="201" analysisType="ReadingFluency"  ><text>Will you help me with the VAST chip security phrases?</text></analysis></AARequest>
         * @param bytes buffer containing the bytes of audio
         * @param secrectKey for client
         * @param transaction ID for this single transaction
         * @param a string stating audio file format
         * @return AA result string sent from speechservices server
         */
        public string singleAATransaction(string authString, string aareqString, byte[] audioBuffer, string clientSecretKey, string transactionID, string audioType)
        {
            string errorMsg = null;
            string validationString = null;
            string aarequestString = null;

            validationString = generateValidationString(authString, clientSecretKey);
            /*Start to Check Inputs*/
            if (null == validationString)
            {
                errorMsg += "Failed in generatin validation string, please check your auth string\n";
                return errorMsg;
            }
            if (null == aareqString)
            {
                errorMsg += "AA Request string should not be empty\n";
                return errorMsg;
            }
            if (audioBuffer.Length <= 0)
            {
                errorMsg += "Audio buffer should not be empty\n";
                return errorMsg;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(audioType, @"(?i)^[a-zA-Z]+$"))
            {
                errorMsg += "audio file type string should be all letters\n";
                return errorMsg;
            }
            if (null == transactionID)
            {
                errorMsg += "Transaction ID should not be null\n";
                return errorMsg;
            }
            /*End of Inputs Checking*/
            if (null != errorMsg)
            {
                return errorMsg;
            }
            aarequestString = aareqString;
            this.sendAARequest(transactionID, validationString, aarequestString, audioType);
            try
            {
                this.sendAudiofileInOneMsg(transactionID, audioBuffer);
            }
            catch (FileNotFoundException e)
            {
                errorMsg += "file not found exception\n";
                Console.WriteLine(e.StackTrace);
                return errorMsg;
            }
            string aaresult = this.recvAAresult();
            return aaresult;
        }
        /* 
         * Used for making a single AA transaction reading in audio data from an audio file and sending with a specified data rate.
         * 
         * @param XML-style authorization string
         * 		Example: <validate requester="CSDev" requesterID="21" requesterInfo='user1' requesterSeqNum='936864273622677291'/>
         * @param XML-style AA request string
         * 		Example: <AARequest AAReqID="201"><analysis analysisID="201" analysisType="ReadingFluency"  ><text>Will you help me with the VAST chip security phrases?</text></analysis></AARequest>
         * @param audio filename string
         * @param secrectKey for client
         * @param transaction ID for this single transaction
         * @param a string stating the data rate: # of bytes per second, string must be string of a positive integer or null
         * @return AA result string sent from speechservices server
         */
        public string singleAATransaction(string authString, string aareqString, string audioFileName, string clientSecretKey, string transactionID, string dataRate)
        {
            if (null == dataRate)
            {
                string returnValue = singleAATransaction(authString, aareqString, audioFileName, clientSecretKey, transactionID);
                return returnValue;
            }
            string errorMsg = null;
            string validationString = null;
            string aarequestString = null;
            string audioType = null;
            /*Start to Check Inputs*/
            if (Int32.Parse(dataRate) <= 0)
            {
                errorMsg += "datarate string must be string of a positive integer \n";
            }
            validationString = generateValidationString(authString, clientSecretKey);
            Console.WriteLine("Validation String is: " + validationString);
            if (null == validationString)
            {
                errorMsg += "Failed in generatin validation string, please check your auth string\n";
            }
            if (null == aareqString)
            {
                errorMsg += "aarequest string should not be empty\n";
            }
            if (null == transactionID)
            {
                errorMsg += "Transaction ID should not be null\n";
            }
            /*End of Inputs Checking*/

            aarequestString = aareqString;
            Console.WriteLine("AArequest String is: " + aarequestString);

            if (!System.IO.File.Exists(audioFileName))
            {
                errorMsg += "Audio file is not existing " + audioFileName + " \n";
            }
            if (null != errorMsg)
            {
                return errorMsg;
            }
            audioType = Path.GetExtension(audioFileName);
            audioType = audioType.Substring(1);
            Console.WriteLine("Audio type is: " + audioType);
            this.sendAARequest(transactionID, validationString, aarequestString, audioType);
            try
            {
                this.sendAudiofileInOneMsg(transactionID, audioFileName, dataRate);
            }
            catch (FileNotFoundException e)
            {
                errorMsg += "file not found exception\n";
                Console.WriteLine(e.StackTrace);
                return errorMsg;
            }
            string aaresult = this.recvAAresult();
            return aaresult;
        }
        /* 
         * Used for making a single AA transaction reading in audio data from an audio file.
         * 
         * @param XML-style authorization string
         * 		Example: <validate requester="CSDev" requesterID="21" requesterInfo='user1' requesterSeqNum='936864273622677291'/>
         * @param XML-style AA request string
         * 		Example: <AARequest AAReqID="201"><analysis analysisID="201" analysisType="ReadingFluency"  ><text>Will you help me with the VAST chip security phrases?</text></analysis></AARequest>
         * @param audio filename string
         * @param secrectKey for client
         * @param transaction ID for this single transaction
         * @return AA result string sent from speechservices server
         */
        public string singleAATransaction(string authString, string aareqString, string audioFileName, string clientSecretKey, string transactionID)
        {
            string errorMsg = null;
            string validationString = null;
            string aarequestString = null;
            string audioType = null;
            /*Start to Check Inputs*/
            validationString = generateValidationString(authString, clientSecretKey);
            if (null == validationString)
            {
                errorMsg += "Failed in generatin validation string, please check your auth string\n";
            }
            if (null == aareqString)
            {
                errorMsg += "aarequest string should not be empty\n";
            }
            if (null == transactionID)
            {
                errorMsg += "Transaction ID should not be null\n";
            }
            /*End of Inputs Checking*/
            aarequestString = aareqString;

            if (!System.IO.File.Exists(audioFileName))
            {
                errorMsg += "Audio file is not existing " + audioFileName + " \n";
            }
            if (null != errorMsg)
            {
                return errorMsg;
            }
            audioType = Path.GetExtension(audioFileName);
            audioType = audioType.Substring(1);
            Console.WriteLine("Audio type is: " + audioType);

            this.sendAARequest(transactionID, validationString, aarequestString, audioType);
            try
            {
                this.sendAudiofileInOneMsg(transactionID, audioFileName);
            }
            catch (FileNotFoundException e)
            {
                errorMsg += "file not found exception\n";
                Console.WriteLine(e.StackTrace);
                return errorMsg;
            }
            string aaresult = this.recvAAresult();
            return aaresult;
        }
        /* 
         * Used for making a single AA transaction reading in audio data from an audio file with validation info as parameters.
         * 
         * @param requester name string
         * 		e.g. "Cambridge-English"
         * @param requesterID string
         * 		e.g. "001"
         * @param userID/assessmentID string
         *      e.g. "Assessment_1234"
         * @param requesterInfo string
         *      e.g. "Question_12"
         * @param requesterSeqNum integer
         *      e.g. "12467"
         * @param aareqString XML-style string
         *      e.g. "<AARequest AAReqID="201"><analysis analysisID="201" analysisType="ReadingFluency"  ><text>Will you help me with the VAST chip security phrases?</text></analysis></AARequest>"
         * @param audio file name string
         *      e.g. "audiofile.flv"
         * @param clientSecretKey string: secrectKey for client
         * @param transactionID string for this single transaction
         *      e.g. "000012354783"
         * @return AA result string sent from speechservices server
         */
        public string singleAATransaction(out bool success, string requester, string requesterID, string userID, string requesterInfo, int requesterSeqNum, string aareqString, string audioFileName, string clientSecretKey, string transactionID)
        {
			success = true;

            string errorMsg = null;
            string validationString = null;
            string audioType = null;
      
            /*Check Inputs*/
            validationString = generateValidationString(requester, requesterID, userID, requesterInfo, requesterSeqNum, clientSecretKey);
            if (null == validationString)
            {
                errorMsg += "Failed in generatin validation string, please check your auth string\n";
            }
            if (null == aareqString)
            {
                errorMsg += "aarequest string should not be empty\n";
            }
            if (null == transactionID)
            {
                errorMsg += "Transaction ID should not be null\n";
            }
			if (!System.IO.File.Exists(audioFileName))
            {
                errorMsg += "Audio file is null. " + audioFileName + " \n";
            }
			if (null != errorMsg)
            {
				success = false;
				Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + errorMsg);
                return errorMsg;
            }
            
			/*Print audio type*/
            audioType = Path.GetExtension(audioFileName);
            audioType = audioType.Substring(1);
            Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + "Audio type is: " + audioType);
            
			/*Sending AA request*/
            success = this.sendAARequest(transactionID, validationString, aareqString, audioType);
			if (success == false)
			{
				errorMsg += "Error in sending AA request \n";
				Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + errorMsg);
				success = false;
                return errorMsg;
			}

			/*Sending audio file*/
            try
            {
                success = this.sendAudiofileInOneMsg(transactionID, audioFileName);
				if (success == false)
				{
					errorMsg += "Error in sending Audio file. \n";
					Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + errorMsg);
					success = false;
					return errorMsg;
				}
            }
            catch (FileNotFoundException e)
            {
                errorMsg += "Error in sending Audio file. \n";
				Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + errorMsg);
                Console.WriteLine("CarnegieBackEnd, singleAATransaction : " + e.StackTrace);
				success = false;
                return errorMsg;
            }
			
            string aaresult = this.recvAAresult();
            return aaresult;
        }
    }
}