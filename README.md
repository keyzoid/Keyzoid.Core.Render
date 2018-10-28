# Keyzoid.Core.Render

Custom pre-rendering solution using PuppeteerSharp for a static website hosted on AWS S3 with source data stored in DynamoDB.

## Getting Started

This solution is highly customized for a particular website. However, the concepts covered and the solution structure may provide value to others struggling with the same problems, namely search engine optimization for websites using early versions of Angular. These instructions will give you an idea of how to customize the solution for your own purposes.

See the following blog posts for some context around the problems I was trying to solve.

* [Part 1](http://jasondkeys.com/blog/server-side-rendering) - Server-side Rendering with .NET Core
* [Part 2](http://jasondkeys.com/blog/pre-render-s3) - Hosting Pre-Rendered Content with AWS S3

### Prerequisites

A Chrome executable, a website URL to be rendered, and either an existing sitemap.xml or content stored in AWS DynamoDB that can be used to create the sitemap. AWS credentials will be needed in the latter case.

### Installing

Update the appsettings.json file with details of the prerequisites above. You can alter the Program.cs steps as needed for your own solution. For example, you can skip the steps for loading site content from DynamoDB to generate the sitemap if you already have one and only need to render.

## Deployment

To run this as part of a CI/CD pipeline, you will need to host this console application on a machine also running Chrome. The Program.cs could be adapted to run on a continuous loop or to be triggered as needed, possibly as a scheduled task.

## Built With

* [.NET Core](https://github.com/dotnet/core) - The console application framework used.
* [Puppeteer Sharp](https://github.com/kblok/puppeteer-sharp) - Puppeteer Sharp is a .NET port of the official Node.JS Puppeteer API.
* [AWS S3](https://github.com/aws/aws-sdk-net/) - Amazon's Simple Storage Solution on which the AngularJS website is hosted.
* [AWS DynamoDB](https://github.com/aws/aws-sdk-net/) - Amazon's managed NoSQL solution used to host the dynamic content for the static website.

## Authors

* **Jason Keys** - *Initial work* - [Keyzoid](http://keyzoid.com)

## Acknowledgments

The following links proved useful on the journey to creating this solution. They provided a guide as to what I did and did not want to do in my solution.

* [Eric Lu](https://www.ericluwj.com/2015/11/17/seo-for-angularjs-on-s3.html) - SEO for AngularJS on S3
* [Prerender.io](https://prerender.io/documentation/test-it) - Middleware you install on a server to check if requests come from a crawler.
* [Zanon](https://zanon.io/posts/angularjs-how-to-create-a-spa-crawlable-and-seo-friendly) - Provided an introduction to PhantomJS which ultimately led to Puppeteer.
* [StackOverflow](https://stackoverflow.com/questions/22383239/single-page-app-amazon-s3-amazon-cloudfront-prerender-io-how-to-set-up) - Thread discussed using Prepender.io in the context of AWS Lambda.
