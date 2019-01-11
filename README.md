# LETS for Orchard

A LETS accounting system based on Orchard https://github.com/OrchardCMS/Orchard

## Setup
1. Download or clone & build
2. Create a database & ensure access
3. Navigate to the website at /src/Orchard.web, select the Default recipe & cook
4. Go to the Dashboard, and enable the LETS module
5. Configure settings as per alerts
6. Create 3 notice types - Offer, Request, Announcement
7. Create some localities
8. Enable the LETS Bootstrap theme
9. Go to taxonomies and add some terms to the Category taxonomy.  You can import some suggested categories from the file in src\Orchard.Web\Modules\LETS\Data\categories.txt
10. Go to Navigation & add links to Offers, Requests & Announcements as follows: Open /Admin/Contents/List/NoticeType in a new tab and hover over the items for Offer, Request, Announcement to discover their ids. Now supposing the Offer notice type has and id of 13, add a Custom Link to the navigation with the Menu text of 'Offers' and a Url /lets/noticetype/list/13.  Do the same for the other 2 notice types.

