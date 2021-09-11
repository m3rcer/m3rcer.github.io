---
title: Grey Hat C#
permalink: /subcategories/csharp/greyhatc/greyhatc
layout: page
excerpt: All post.
comments: false
---




<p align="center">
     <img src="https://libribook.com/Images/gray-hat-c-pdf.jpg">
</p>

* [View/Download(*I do not own this*)](https://drive.google.com/file/d/0B4hhbFaItiPxY0FNbG4ycFNxcXM/view?resourcekey=0-IyY1FRkKjxsaz8tZQpxLuw)

* Support the Authors(Brandon Perry, Matt Graeber) and grab a copy yourself if u can. 

* For hacker's with a mild understanding of programming like python / C++ and an urge to develop C# skills for Red Teaming and creating the latest offensive hacking tradecraft.

\*_Not complete yet, under active dev_*


## Contents:


<h1>1. Introductory scripts.</h1>
{%- for post in site.categories.greyhatcch1 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}


<h1>2. Fuzzing and exploiting sql/xss.</h1>
{%- for post in site.categories.greyhatcch2 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}

<h1>3. Fuzzing SOAP endpoints.</h1>
{%- for post in site.categories.greyhatcch3 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}


<h1>4. Writing Connect-Back, Binding and Metasploit payloads.</h1>
{%- for post in site.categories.greyhatcch4 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}

<h1>5. Automating Nessus.</h1>
{%- for post in site.categories.greyhatcch5 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}

<h1>5. Automating Nexpose.</h1>
{%- for post in site.categories.greyhatcch6 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}


<h1>6. Automating OpenVAS.</h1>
{%- for post in site.categories.greyhatcch7 reversed-%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}


### 6. Automating Nexpose 

- [Nexpose API Automation](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch6/README.md)

### 7. Automating OpenVAS 

- [Nexpose API Automation](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch7/README.md)