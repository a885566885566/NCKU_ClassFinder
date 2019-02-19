#!/usr/bin/python3
import requests
import smtplib
import time
import json
from bs4 import BeautifulSoup
from email.mime.text import MIMEText

def send_email(username, passwd, receiver, subject, message):
    msg = MIMEText(message)
    msg['From'] = username
    msg['To'] = receiver
    msg['Subject'] = subject
    
    server = smtplib.SMTP_SSL('smtp.gmail.com', 465)
    server.ehlo()
    server.login(username, passwd)
    server.send_message(msg)
    server.quit()

"""
@param: dep_id: The department id.
Download the html page and parse into bs4 object.
"""
def get_page_soup(dep_id):
   # To cheat the server as browser's request'
   headers = {'user-agent': 'Mozilla/5.0 (Macintosh Intel Mac OS X 10_13_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36'}
   r = requests.get('http://course-query.acad.ncku.edu.tw/qry/qry001.php?dept_no=' + dep_id, headers=headers)
   if r.status_code == 200:
        # For Chinese elements
        r.encoding = 'utf-8'
        html_doc = r.text
        soup = BeautifulSoup(html_doc, 'html.parser')
        return soup
   else:
        return none
"""
@parm: course_id: The id of the class.
@return: Object course object of the target course id
This function will download the html page from NCKU course look-up system and parse it with beautifulsoup.
"""
def get_course_obj(courseInfo, target_id):
        # Examine if there exist a class match the target id
        for detail in courseInfo:
            if detail['id'] == target_id:
                return detail

"""
@param: soup: The page tree object(beautifulsoup)
@return: [{Object}{Object}] Course infomation array 
This function parse the course infomation according to NCKU course look-up system's table arrangement. It returns a array containing course objects with attribures of id, code, name and left.
"""
def get_td_content(soup):
    courses = []
    # A <tr> element contains infomation of a course
    #rows = soup.find_all(name='tr', class_='course_y'+year)
    rows = soup.find_all(name='tr')
    for row in rows:
        arr = []
        info = {}
        # The infomation of A course is represent with <td> objects
        column = row.find_all(name='td')
        for td in column:
            txt = td.get_text()
            if txt != None:
                arr.append(txt)
            else:
                arr.append(td)
        # A normal course containing at least 17 elements and first element should not be nonetype
        if len(arr) > 17 and arr[1]:
            info['id'] = arr[1] + arr[2]
            info['code'] = arr[3]
            info['name'] = arr[10]
            info['left'] = arr[15]
            info['time'] = str(arr[16])
            courses.append(info)
    return courses

def get_dep_id(course_id):
    return course_id[:2]

def get_balance(target_list):
    target_list.sort()
    detail = []
    last_dep = 'none'
    for target in target_list:
        dep_id = get_dep_id(target)
        if last_dep != dep_id:
            page_soup = get_page_soup(dep_id)
            # Extrade the course infomation with the page tree
            courseInfo = get_td_content(page_soup)
            last_dep = dep_id
        course = get_course_obj(courseInfo, target)
        detail.append(course)
    return detail

"""
Determine whether two object are equal.
"""
def check_object(arr_a, arr_b):
    if len(arr_a) != len(arr_b):
        return False
    for a, b in zip(arr_a, arr_b):
        if a != b:
            return False
    return True

def pretty_print(course):
    msg = ""
    for item in course:
        msg = msg+item['id']+item['name']+item['time']+"= "+item['left']+'\n'
    return msg

def main():
    with open('config.json') as f:
        config = json.load(f)
        course = get_balance(config["target"])
        msg = 'Class Tracker Startup. Tracking course are ' + str(config["target"]) + '\n\n'
        msg = msg + pretty_print(course)
        print('Sending email...')
        send_email(username= config["username"],
                   passwd= config["passwd"],
                   receiver= config["receiver"],
                   subject= 'Class Tracker Startup',
                   message= msg)
        course_state = course
        print('OKay')
        while True:
            try:
                course = get_balance(config["target"])
                print('Found class:\n'+pretty_print(course))
                if not check_object(course, course_state):
                    send_email(username= config["username"],
                               passwd= config["passwd"],
                               receiver= config["receiver"],
                               subject= 'Class Tracker Found Balance',
                               message= pretty_print(course))
                    course_state = course
                time.sleep(10)
            except ConnectionError:
                print('Connection Error')
    
if __name__ == "__main__":
    main()
