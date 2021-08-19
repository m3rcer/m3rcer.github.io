---
title: Python 101 For Hackers
permalink: /subcategories/python/python_101/python_101
layout: page
excerpt: All post.
comments: false
---

{%- for post in site.categories.python_101 reversed -%}
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