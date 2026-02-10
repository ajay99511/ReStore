import { Box, Typography } from "@mui/material";
import Slider from "react-slick";
export default function(){
    var settings = {
        dots: true,
        infinite: true,
        speed: 500,
        slidesToShow: 1,
        slidesToScroll: 1
      };
    return(
        <>
        <Slider {...settings}>
        <div>
            <img src="/images/hero1.jpg" alt="hero1" style={{display:'block',width:'100%',maxHeight:400}}/>
        </div>
        <div>
            <img src="/images/hero2.jpg" alt="hero2" style={{display:'block',width:'100%',maxHeight:400}}/>
        </div>
        <div>
            <img src="/images/hero3.jpg" alt="hero3" style={{display:'block',width:'100%',maxHeight:400}}/>
        </div>
        </Slider>
            
        <Box justifyContent='center' sx={{p:4}}>
        <Typography variant="h2" color="secondary">
           Welcome to the Speed-Cart!
        </Typography>
        <Typography variant="h3" color= "primary" mt={2}>
        Get the things you want much Faster & Better
        </Typography>
        </Box>
        {/* <UserGrid/> */}
     </>
    
    )
}