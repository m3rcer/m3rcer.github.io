---
title: CSharp Craft
permalink: /subcategories/csharp/csharp
layout: page
excerpt: All post.
comments: false
---


	            *********
	           *************
	          *****     *****
	         ***           ***
	        ***             ***
	        **    C     #    **
	        **               **                  ____
	        ***             ***             //////////
	        ****           ****        ///////////////  
	        *****         *****    ///////////////////
	        ******       ******/////////         |  |
	      *********     ****//////               |  |
	   *************   **/////*****              |  |
	  *************** **///***********          *|  |*
	 ************************************    ****| <=>*
	*********************************************|<===>* 
	*********************************************| <==>*
	***************************** ***************| <=>*
	******************************* *************|  |*
	********************************** **********|  |*  .NETR3AP3R  
	*********************************** *********|  |

# [Grey Hat C#](/subcategories/csharp/greyhatc/greyhatc)

{%- for post in site.categories.grayhatc -%}
	  {%- capture current_year -%}{{ post.date | date: "%Y" }}{%- endcapture -%}
	  {%- unless current_year == previous_year -%}
	    <h2>{{ current_year }}</h2>
	    {%- assign previous_year = current_year -%}
	  {%- endunless -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}

