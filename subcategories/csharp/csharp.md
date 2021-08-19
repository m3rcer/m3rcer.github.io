---
title: CSharp Craft
permalink: /subcategories/csharp/csharp.md
layout: page
excerpt: All post.
comments: false
---


<p align="center">
 <img src="https://user-images.githubusercontent.com/29004603/75462714-9dd79b80-59bf-11ea-8e6b-575765733340.png" width="300" height="200">
</p>

# Grey Hat C#

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

