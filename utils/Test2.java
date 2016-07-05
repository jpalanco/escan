import java.io.*;
import java.util.*;
import javax.servlet.*;
import javax.servlet.http.*;

public class Test2 extends HttpServlet {

    public void doGet(HttpServletRequest request, HttpServletResponse response) throws IOException, ServletException
    {
        PrintWriter out = response.getWriter();
        String tmp = request.getParameter("lastname");
        String aaa = "pepe";
        out.println(tmp);
        out.println(request.getParameter("lastname"));
    }

}