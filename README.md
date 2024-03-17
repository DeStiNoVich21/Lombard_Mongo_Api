This is the test project in our work practic , the main api for https://github.com/MorozovTimofey/LombNet.
Features:
1)Uses generic repository
2)Works on MongoDB
3)Can hash the password and verify it
4)Uses JWT token for login
5)Can store Images
6)Despite the foreign key isn't featured in Mongo DB we made some functions work like its using foreign key  

This is our first team project and we dont have plans upgrading it afterwards , so it's kind like a little reminder of our deeds :)

Short instructions for its functionality:
1)Authorization: It`s allows for login and registration 
  1.1) Login works by creating a jwt token and writes user_id and ClaimTypes.Roles
  1.2) Registration uses password hashing and stores password hash and password salt in DB
  1.3) Registration creates only users
  1.4) The 'Username' must unique
  1.5) Token lasts for 3 hours straight
  1.6) You create and control Moderators&Users and Lombards through the controllers(For admin)
2)Products: To add the Lombards products
  2.1) You can create you own product category
  2.2) The photo of the product is stored directly in MongoDB
  2.3) The admin can fully delete the products, but moderator can only change 'isdeleted' field 
3)Transactions: For users to buy some products
  3.1) Uses the 'status' field to determine its status
  3.2) The collection contains only the 'UserID',"ProductID","TransactionID","Status"
4)Lombards: For admin, allows to create and control the lombards
  4.1)If the lombard is deleted the products containing the 'LombardID' field same as the lombard id will be deleted too
  4.2)The lombard can contain only ONE of each product
  4.3)The lombard can control its products list, categories


With best regards ICEY_YOU , date: 20:48 of 17.03.2024
