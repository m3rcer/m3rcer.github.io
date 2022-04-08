<h1 align="center">Engaging and pentesting various SMTP relays directly</h1>


<p align="center">
     <img src="https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/smtpmeme.png">
</p>

_________________________________________________________________________________________________

- The default ports used are 25, 465 and 587, where 25 is meant to be used for submissions from your e-mail client to the e-mail server and the higher ports for relaying between SMTP-server.

- Some terms assocaited with SMTP:

  > Mail User Agent (MUA): This is a (part of a) program connecting to a SMTP-server in order to send an email. Most likely this is your Outlook, Thunderbird, whatever.

  > Mail Transfer Agent (MTA): The transport service part of a program. They receive and transfer the emails. This might be an Exchange server, an internet facing gateway and so on.

- SMTP Commands:


```bash
   HELO: It’s the very first SMTP welcome command to start the conversation and identifying the sender server and is followed by its domain name.

   EHLO: Same like HELO command or An alternative command to start the conversation, underlying that the server is using the Extended SMTP protocol.

   EMAIL: FROM: The sender states the source email address in the “From” field and starts the email transfer.

   RCPT: TO: It identifies the recipient of the email.

   DATA: Sending your body of mail by the DATA command the email content begins to be transferred.

   VRFY: The server is asked to verify whether a particular email address or username exists.

   AUTH: Authentication command, the client authenticates itself to the server, giving its username and password.

   HELP: client’s request for some information that can be useful for the successful transfer of the email.

   EXPN: asks for confirmation about the identification of a mailing list.

   RSET: Client communicates the server to stop the ongoing email transmission or terminating the continuous mail from the server.

   ETRN: or TURN changes roles between the client and the server.Client will be acting as SMTP Server.

   QUIT: It terminates the SMTP conversation.
```

- A workflow of an email´s travel from one user to another would look like so:

  `MUA → MSA → MTA → internet → MTA → MDA → MUA`.

- A "relay" SMTP system receives mail from an SMTP client and transmits it, without modification to the message data other than adding trace information, to another SMTP server for further relaying or for delivery.

_________________________________________________________________________________________________


# Attacking the SMTP-Server:

## Various Scenarious:

1. Someone connects to your SMTP-server and wants to send from an account on your domain to another on your domain.
   
   This is the “normal” case that one would assume to happen on a daily basis. 
   Hence it is _abusable_ if creds are available.

2. Someone connects to your SMTP-server and wants to send from your domain to an external domain.

  I would consider this to be at least unusual, as no one from your organisation will directly transfer their emails to the firewall under normal circumstances. 
  Hence it is _not as abusable_.

3. Someone connects to your SMTP-server and wants to send from an external domain to an external domain.

   This would be considered an open mail-relay if allowed and you don´t want that to happen, unless you like yourself to be put on all the blacklists for spammers out there.
   Hence it is _Higly Abusable_.

_________________________________________________________________________________________________

## Hacking:

1. Using a simple nslookup for the MX records:

```bash
nslookup
set type=mx
example.com
```

The result yields systems responsible for incoming mail for that domain.

![Image](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/direct1.png)


2. Next we perform a basic nmap scan, to identify open ports: Namely seeking ports - 25,465,587.

![Image](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/direct2.png)


- Now based on what ports are open we can perform the following:

  > Connecting/Pentesting Port 25:

    - Mailserver User Enumeration: use the VRFY/EXPN SMTP Command.

    ```bash
      telnet 10.0.0.1 25
      Trying 10.0.0.1...
      Connected to 10.0.0.1.
      Escape character is '^]'.
      220 myhost ESMTP Sendmail 8.9.3
      HELO
      501 HELO requires domain address
      HELO x
      250 myhost Hello [10.0.0.99], pleased to meet you
      VRFY boobman
      250 Super-User <boobman@myhost>
      VRFY blah
      550 blah... User unknown
    ```

    ```bash
      telnet 10.0.10.1 25
      Trying 10.0.10.1...
      Connected to 10.0.10.1.
      Escape character is '^]'.
      220 myhost ESMTP Sendmail 8.9.3
      HELO
      501 HELO requires domain address
      HELO x
      EXPN test
      550 5.1.1 test... User unknown
      EXPN root
      250 2.1.5 <ed.williams@myhost>
      EXPN sshd
      250 2.1.5 sshd privsep <sshd@mail2>
    ```

  - Sending email:

  Use the SMTP Commands 'MAIL FROM' and 'RCPT TO' to send the forged mail to the receiver using:

   > Syntax for sender: _mail from: 'kingkong@example.com'_

   > Syntax for receiver: _rcpt to: 'tarzan@example.com'_

   > Utilize the SMTP command 'DATA' to compose the mail in the command line.


  
   __Pentesting port 465/587(SPF,DKIM,DMARC):__
   

    - Use https://github.com/serain/mailspoof to check for SPF and DMARC misconfigurations.

      Traditionally it was possible to spoof any domain name that didn't have a correct/any SPF record. Nowadays, if email comes from a domain without a valid SPF record is probably going to be rejected/marked as untrusted automatically and hence most have all the checks - SPF,DMARC,DKIM at play making abusable attacks like open relays less common unless certaing misconfigurations come at play.

    - An email spoofing testing tool that aims to bypass SPF/DKIM/DMARC and forge DKIM signatures: https://github.com/chenjj/espoofer.

_________________________________________________________________________________________________

## Conclusion:

If configured unsafe, SMTP-servers can put your company at a high risk. You neither want external parties to send emails from your domain to your domain without authentication, nor do you want your SMTP-server to act as an open mail-relay.

Make use of the security mechanisms that are available to protect your environment(SPF,DKIM,DMARC,SSL/TLS) as much as possible.

_________________________________________________________________________________________________
