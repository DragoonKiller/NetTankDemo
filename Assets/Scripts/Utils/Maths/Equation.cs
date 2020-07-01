/***************************************************************************
 *   Copyright (C) 2016 by Саша Миленковић                                 *
 *   sasa.milenkovic.xyz@gmail.com                                         *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *   ( http://www.gnu.org/licenses/gpl-3.0.en.html )                       *
 *									   *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
 ***************************************************************************/

using System;
using Utils;
using UnityEngine;
using System.Numerics;

namespace Utils
{

    public static class Equation
    {
        
            public static (float? x1, float? x2, float? x3) Solve3(float a, float b, float c, float d) 
            {
                return Solve3(b / a, c / a, d / a);
            }
            
            /// https://github.com/sasamil/Quartic/blob/master/quartic.cpp
            /// 解三次方程 x^3 + a*x^2 + b*x + c
            /// 只求实根.
            // In case 3 real roots: => x[0], x[1], x[2], return 3
            //         2 real roots: x[0], x[1],          return 2
            //         1 real root : x[0], x[1] ± i*x[2], return 1
            public static (float? x1, float? x2, float? x3) Solve3(float a, float b, float c) 
            {
                float a2 = a * a;
                float q  = (a2 - 3 * b) / 9;
                float r  = (a * (2 * a2 - 9 * b) + 27 * c) / 54;
                float r2 = r * r;
                float q3 = q * q * q;
                float A, B;
                if(r2 < q3) 
                {
                    float t = r / q3.Sqrt();
                    if(t < -1) t = -1;
                    if(t > 1) t = 1;
                    t = t.Acos();
                    a /= 3;
                    q = -2 * q.Sqrt();
                    return (q * (t / 3f).Cos() - a,
                        q * ((t + Mathf.PI) / 3).Cos() - a,
                        q * ((t - Mathf.PI) / 3).Cos() - a
                    );
                } 
                else 
                {
                    A = -(r.Abs() + (r2 - q3).Sqrt()).Pow(1f / 3);;
                    if(r < 0) A = -A;
                    B = (0 == A ? 0 : q / A);
                    a /= 3;
                    var (x1, x2, x3) = (
                        (A + B) - a,
                        -0.5f * (A + B) - a,
                        0.5f * 3f.Sqrt() * (A - B)
                    );
                    if(x2.Abs() < Maths.eps) return ( x1, x3, null );
                    else return ( x1, null, null );
                }
            }
        
        public static (float? x1, float? x2, float? x3,  float? x4) Solve4(float a, float b, float c, float d, float e)
        {
            return Solve4(b / a, c / a, d / a, e / a);
        }
        
        /// https://github.com/sasamil/Quartic/blob/master/quartic.cpp
        // 解方程 0 == x^4 + a*x^3 + b*x^2 + c*x + d
        // 只求实根.
        public static (float? x1, float? x2, float? x3,  float? x4) Solve4(float a, float b, float c, float d)
        {
            unsafe
            {
            
                float a3 = -b;
                float b3 =  a * c - 4f * d;
                float c3 = -a * a * d - c * c + 4f * b * d;

                // cubic resolvent
                // y^3 − b*y^2 + (ac − 4d) * y − a^2 * d − c^2 + 4*b*d = 0
                var (sx1, sx2, sx3) = Solve3(a3, b3, c3);
                var virs = (sx1 == null ? 0 : 1)
                    + (sx1 == null ? 0 : 1)
                    + (sx1 == null ? 0 : 1);
                
                float q1, q2, p1, p2, D, sqD, y;
                
                var x = stackalloc float[3];
                x[0] = sx1 == null ? 0 : sx1.Value;
                x[1] = sx2 == null ? 0 : sx2.Value;
                x[2] = sx3 == null ? 0 : sx3.Value;
                
                y = x[0];
                // The essence - choosing Y with maximal absolute value.
                if(virs != 1)
                {
                    if(x[1].Abs() > y.Abs()) y = x[1];
                    if(x[2].Abs() > y.Abs()) y = x[2];
                }

                // h1+h2 = y && h1*h2 = d  <=>  h^2 -y*h + d = 0    (h === q)

                D = y * y - 4 * d;
                if(D.Abs() < Maths.eps) //in other words - D==0
                {
                    q1 = q2 = y * 0.5f;
                    // g1+g2 = a && g1+g2 = b-y   <=>   g^2 - a*g + b-y = 0    (p === g)
                    D = a * a - 4 * (b - y);
                    if(D.Abs() < Maths.eps) //in other words - D==0
                        p1 = p2 = a * 0.5f;

                    else
                    {
                        sqD = D.Sqrt();
                        p1 = (a + sqD) * 0.5f;
                        p2 = (a - sqD) * 0.5f;
                    }
                }
                else
                {
                    sqD = D.Sqrt();
                    q1 = (y + sqD) * 0.5f;
                    q2 = (y - sqD) * 0.5f;
                    // g1+g2 = a && g1*h2 + g2*h1 = c       ( && g === p )  Krammer
                    p1 = (a * q1 - c) / (q1 - q2);
                    p2 = (c - a * q2) / (q1 - q2);
                }

                // var retval = stackalloc Complex[4];
                float? x1 = null, x2 = null, x3 = null, x4 = null;
                
                // solving quadratic eq. - x^2 + p1*x + q1 = 0
                D = p1 * p1 - 4 * q1;
                if(D < 0.0)
                {
                    // retval[0] = new Complex( -p1 * 0.5, (-D).Sqrt() * 0.5 );
                    // retval[1] = Complex.Conjugate(retval[0]);
                }
                else
                {
                    sqD = D.Sqrt();
                    x1 = (-p1 + sqD) * 0.5f;
                    x2 = (-p1 - sqD) * 0.5f;
                }

                // solving quadratic eq. - x^2 + p2*x + q2 = 0
                D = p2 * p2 - 4 * q2;
                if(D < 0.0)
                {
                    // retval[2] = new Complex( -p2 * 0.5, (-D).Sqrt() * 0.5 );
                    // retval[3] = Complex.Conjugate(retval[2]);
                }
                else
                {
                    sqD = D.Sqrt();
                    // retval[2] = new Complex((-p2 + sqD) * 0.5, 0);
                    // retval[3] = new Complex((-p2 - sqD) * 0.5, 0);
                    x3 = (-p2 + sqD) * 0.5f;
                    x4 = (-p2 + sqD) * 0.5f;
                }
                
                return x1 != null ? (x1, x2, x3, x4) : (x3, x4, x1, x2);
            }
        }

    }
}
